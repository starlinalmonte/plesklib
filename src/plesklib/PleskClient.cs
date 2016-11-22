﻿namespace plesklib
{
    using plesklib.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class PleskClient
    {
        private string apiurl;
        private string hostname;
        private string port;
        private string username;
        private string password;

        public PleskClient()
        {

        }

        public PleskClient(string hostname, string username, string password, string port = "8443", bool https = true)
        {
            this.hostname = hostname;
            this.username = username;
            this.password = password;
            this.port = port;
            
            this.apiurl = String.Format("{2}://{0}:{1}/enterprise/control/agent.php", hostname, port, https ? "https" : "http");
        }

        private void Auth(ref HttpWebRequest req)
        {
            req.Timeout = 30000;
            req.Method = "POST";
            req.Headers.Add("HTTP_AUTH_LOGIN", username);
            req.Headers.Add("HTTP_AUTH_PASSWD", password);
            req.ContentType = "text/xml";
        }

        public string SerializeObjectToXmlString<T>(T TModel)
        {
            string xmlData = String.Empty;

            var encoding = new UTF8Encoding(false);    

            XmlSerializerNamespaces EmptyNameSpace = new XmlSerializerNamespaces();
            EmptyNameSpace.Add("", "");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(memoryStream, encoding);
            xmlWriter.Formatting = Formatting.Indented;

            xmlSerializer.Serialize(xmlWriter, TModel, EmptyNameSpace);

            memoryStream = (MemoryStream)xmlWriter.BaseStream;
            xmlData = encoding.GetString(memoryStream.ToArray());

            return xmlData;
        }

        public T DeSerializeObject<T>(string xmlString)
        {
            T deSerializeObject = default(T);

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var stringReader = new StringReader(xmlString))
            {
                var XR = new XmlTextReader(stringReader);
                
                if (xmlSerializer.CanDeserialize(XR))
                {
                    deSerializeObject = (T)xmlSerializer.Deserialize(XR);
                }
            }
        
            return deSerializeObject;
        }

        private string SendHttpRequest(string meesage)
        {
            var result = String.Empty;
            var bytes = new ASCIIEncoding().GetBytes(meesage);            

            //Bypass SSL validation.
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate
                                                                            (object sender, X509Certificate certificate, X509Chain chain,
                                                                            SslPolicyErrors sslPolicyErrors)
            { return true; };
            
            var request = (HttpWebRequest)WebRequest.Create(this.apiurl);
            Auth(ref request);            
            request.ContentLength = meesage.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();                
            }

            result = GetResponseContent(request);
            return result;
        }

        private string GetResponseContent(HttpWebRequest request)
        {
            using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private Toutput ExecuteWebRequest<Tinput, Toutput>(Tinput apiRequest)
        {
            var response = new ApiResponse();
            response.Status = false;

            var result = Activator.CreateInstance(typeof(Toutput));

            try
            {
                var message = SerializeObjectToXmlString<Tinput>(apiRequest);
                response.ResponseXmlString = SendHttpRequest(message);                
                result = DeSerializeObject<Toutput>(response.ResponseXmlString);

                response.Status = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.MessageDetails = ex.StackTrace;
            }

            if (!response.Status)
            {
                var output = result as IResponseResult;

                if(output != null)
                    output.SaveResult(response);                
            }

            return (Toutput)result;
        }

        #region Actions
        public SiteAddResult CreateSite(string name, string webspaceid, HostingProperty[] properties)
        {              
            var prop = new List<HostingProperty>();

            if(properties != null)
                prop.AddRange(properties);
            
            var add = new SiteAddPacket();
            add.Site.Add.GenSetup.Name = name;
            add.Site.Add.GenSetup.WebSpaceId = webspaceid;
            add.Site.Add.Hosting.Properties = prop.ToArray();

            return ExecuteWebRequest<SiteAddPacket, SiteAddResult>(add);            
        }



        public SiteGetResult GetSite(string name)
        {
            var get = new SiteGet();
            get.filter.Name = name;

            return ExecuteWebRequest<SiteGet, SiteGetResult>(get);
        }

        public SiteAliasPacketResult CreateAlias(int siteId, string name)
        {
            var add = new SiteAliasPacket();
            add.siteAlias.createSiteAlias.SiteId = siteId;
            add.siteAlias.createSiteAlias.AliasName = name;

            return ExecuteWebRequest<SiteAliasPacket, SiteAliasPacketResult>(add);
        }
        #endregion
    }
}
