using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using System.Net;
using System.Xml;

namespace CentraleRischiR2Library
{
    public class TextOrMtomEncodingBindingElement : MessageEncodingBindingElement
    {
        public const string IsIncomingMessageMtomPropertyName = "IncomingMessageIsMtom";
        public const string MtomBoundaryPropertyName = "__MtomBoundary";
        public const string MtomStartInfoPropertyName = "__MtomStartInfo";
        public const string MtomStartUriPropertyName = "__MtomStartUri";

        MessageVersion messageVersion = MessageVersion.Default;

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new TextOrMtomEncoderFactory(this.messageVersion);
        }
        public override MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
            set
            {
                this.messageVersion = value;
            }
        }
        public override BindingElement Clone()
        {
            return this;
        }
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelFactory<TChannel>();
        }
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }
    class TextOrMtomEncoderFactory : MessageEncoderFactory
    {
        MessageVersion messageVersion;
        TextOrMtomEncoder encoder;

        public TextOrMtomEncoderFactory(MessageVersion messageVersion)
        {
            this.messageVersion = messageVersion;
            this.encoder = new TextOrMtomEncoder(messageVersion);
        }
        public override MessageEncoder Encoder
        {
            get
            {
                return this.encoder;
            }
        }
        public override MessageVersion MessageVersion
        {
            get
            {
                return this.encoder.MessageVersion;
            }
        }
    }
    class TextOrMtomEncoder : MessageEncoder
    {
        MessageEncoder _textEncoder;
        MessageEncoder _mtomEncoder;
        public TextOrMtomEncoder(MessageVersion messageVersion)
        {
            _textEncoder = new TextMessageEncodingBindingElement(messageVersion, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
            _mtomEncoder = new MtomMessageEncodingBindingElement(messageVersion, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
        }
        public override string ContentType
        {
            get
            {
                return _textEncoder.ContentType;
            }
        }
        public override string MediaType
        {
            get
            {
                return _textEncoder.MediaType;
            }
        }
        public override MessageVersion MessageVersion
        {
            get
            {
                return _textEncoder.MessageVersion;
            }
        }
        public override bool IsContentTypeSupported(string contentType)
        {
            return this._mtomEncoder.IsContentTypeSupported(contentType);
        }
        public override T GetProperty<T>()
        {
            T result = this._textEncoder.GetProperty<T>();
            if (result == null)
            {
                result = this._mtomEncoder.GetProperty<T>();
            }
            if (result == null)
            {
                result = base.GetProperty<T>();
            }
            return result;
        }
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            // FG - CervedGroup

            //Convert the received buffer into a string
            byte[] incomingResponse = buffer.Array;


            //read the first 500 bytes of the response
            string strFirst500 = System.Text.Encoding.UTF8.GetString(incomingResponse, 0, 500);

            /*
            * Check the last occurance of 'application/xop+xml' in the response. We check for the last 
            * occurrence since the first one is present in the Content-Type HTTP Header. Once found, 
            * append charset header to this string if it is not present
            */
            int appIndex = strFirst500.LastIndexOf("application/xop+xml");
            int appIndexCharsetUTF8 = strFirst500.LastIndexOf("charset=utf-8");
            if (appIndex != -1 && appIndexCharsetUTF8 == -1)
            {
                String modifiedResponse = strFirst500.Insert(appIndex + 19, ";charset=utf-8");


                //convert the modified string back into a byte array
                byte[] ma = System.Text.Encoding.UTF8.GetBytes(modifiedResponse);


                //integrate the modified byte array back to the original byte array
                int increasedLength = ma.Length - 500;
                byte[] newArray = new byte[incomingResponse.Length + increasedLength];


                for (int count = 0; count < newArray.Length; count++)
                {
                    if (count < ma.Length)
                    {
                        newArray[count] = ma[count];
                    }
                    else
                    {
                        newArray[count] = incomingResponse[count - increasedLength];
                    }
                }


                /*
                * In this part generate a new ArraySegment<byte> buffer and pass it to the underlying MTOM 
                * Encoder.
                */
                int size = newArray.Length;
                byte[] msg = bufferManager.TakeBuffer(size);
                Array.Copy(newArray, msg, size);
                ArraySegment<byte> newResult = new ArraySegment<byte>(msg);

                Message result = this._mtomEncoder.ReadMessage(newResult, bufferManager, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
            else
            {

                Message result = this._mtomEncoder.ReadMessage(buffer, bufferManager, contentType);
                result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
                return result;
            }
        }
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            if (this.ShouldWriteMtom(message))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    XmlDictionaryWriter mtomWriter = CreateMtomWriter(ms, message);
                    message.WriteMessage(mtomWriter);
                    mtomWriter.Flush();
                    byte[] buffer = bufferManager.TakeBuffer((int)ms.Position + messageOffset);
                    Array.Copy(ms.GetBuffer(), 0, buffer, messageOffset, (int)ms.Position);
                    return new ArraySegment<byte>(buffer, messageOffset, (int)ms.Position);
                }
            }
            else
            {
                return this._textEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
            }
        }
        private static bool IsMtomMessage(string contentType)
        {
            return contentType.IndexOf("type=\"application/xop+xml\"", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool ShouldWriteMtom(Message message)
        {
            object temp;
            return message.Properties.TryGetValue(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, out temp) && (bool)temp;
        }
        private XmlDictionaryWriter CreateMtomWriter(Stream stream, Message message)
        {
            string boundary = message.Properties[TextOrMtomEncodingBindingElement.MtomBoundaryPropertyName] as string;
            string startUri = message.Properties[TextOrMtomEncodingBindingElement.MtomStartUriPropertyName] as string;
            string startInfo = message.Properties[TextOrMtomEncodingBindingElement.MtomStartInfoPropertyName] as string;
            return XmlDictionaryWriter.CreateMtomWriter(stream, Encoding.UTF8, int.MaxValue, startInfo, boundary, startUri, false, false);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            Message result = this._mtomEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
            result.Properties.Add(TextOrMtomEncodingBindingElement.IsIncomingMessageMtomPropertyName, IsMtomMessage(contentType));
            return result;
        }
        public override void WriteMessage(Message message, Stream stream)
        {
            if (ShouldWriteMtom(message))
            {
                XmlDictionaryWriter mtomWriter = CreateMtomWriter(stream, message);
                message.WriteMessage(mtomWriter);
            }
            else
            {
                _textEncoder.WriteMessage(message, stream);
            }
        }
    }
}

