using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SketchFab
{
    public class SketchFab
    {
        private const string SKETCHFAB_API_URL = "https://api.sketchfab.com/v2/models";
        private static readonly Encoding ENCODING = Encoding.UTF8;
        private const string USER_AGENT = "RhinoIn";

        /// <summary>
        /// Sketchfab account token (mondatory field for upload)
        /// </summary>
        private string _token;

        public SketchFab(string token)
        {
            this._token = token;
        }

        public string Token
        {
            get
            {
                return this._token;
            }
        }

        public HttpWebResponse UploadModel(byte[] modelFile, string fileName, string fileExtention)
        {
            string postUrl = SKETCHFAB_API_URL;
            string postBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + postBoundary;

            return PostModel(postUrl, USER_AGENT, contentType, GetPostData(GetPostParameters(modelFile, fileName, fileExtention, null, null, null, false, null), postBoundary));
        }

        public HttpWebResponse UploadModel(byte[] modelFile, string fileName, string fileExtention, string modelName, string modelDescription, string modelTags, bool isModelPrivate, string accountPassword)
        {
            string postUrl = SKETCHFAB_API_URL;
            string postBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + postBoundary;

            return PostModel(postUrl, USER_AGENT, contentType, GetPostData(GetPostParameters(modelFile, fileName, fileExtention, modelName, modelDescription, modelTags, isModelPrivate, accountPassword), postBoundary));
        }

        public string UploadResult(WebResponse modelUploadResponse)
        {
            StreamReader responseReader = new StreamReader(modelUploadResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            modelUploadResponse.Close();
            return fullResponse;
        }

        private Dictionary<string, object> GetPostParameters(byte[] modelFile, string fileName, string fileExtention, string modelName, string modelDescription, string modelTags, bool isModelPrivate, string accountPassword)
        {
            Dictionary<string, object> postParameters = new Dictionary<string, object>();
            postParameters.Add("token", this._token);
            postParameters.Add("modelFile", new ModelFile(modelFile, fileName, fileExtention));

            if (modelName != null)
            {
                postParameters.Add("name", modelName);
            }

            if (modelDescription != null)
            {
                postParameters.Add("description", modelDescription);
            }

            if (modelTags != null)
            {
                postParameters.Add("tags", modelTags);
            }

            if (isModelPrivate == true && accountPassword != null)
            {
                postParameters.Add("private", true);
                postParameters.Add("password", accountPassword);
            }

            return postParameters;
        }

        private byte[] GetPostData(Dictionary<string, object> postParameters, string postBoundary)
        {
			Stream postDataStream = new MemoryStream();
			bool needsCLRF = false;

            foreach (var param in postParameters)
			{
                Console.WriteLine(param.Key);
				if (needsCLRF)
				{
                    postDataStream.Write(ENCODING.GetBytes("\r\n"), 0, ENCODING.GetByteCount("\r\n"));
				}

				needsCLRF = true;

				if (param.Value is ModelFile)
				{
					ModelFile fileToUpload = (ModelFile)param.Value;

					// Add just the first part of this param, since we will write the file data directly to the Stream
					string header =
					    string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", postBoundary, param.Key, fileToUpload.FileName, fileToUpload.ContentType);
                    Console.WriteLine(header);
                    postDataStream.Write(ENCODING.GetBytes(header), 0, ENCODING.GetByteCount(header));

					// Write the file data directly to the Stream, rather than serializing it to a string.
					postDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
				}
				else
				{
					string content =
					    string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", postBoundary, param.Key, param.Value);
                    Console.WriteLine(content);
                    postDataStream.Write(ENCODING.GetBytes(content), 0, ENCODING.GetByteCount(content));
				}
			}

			// Add the end of the request. Start with a newline
			string footer = "\r\n--" + postBoundary + "--\r\n";
            postDataStream.Write(ENCODING.GetBytes(footer), 0, ENCODING.GetByteCount(footer));

			// Dump the Stream into a byte[]
			postDataStream.Position = 0;
			byte[] postData = new byte[postDataStream.Length];
			postDataStream.Read(postData, 0, postData.Length);
			postDataStream.Close();

            return postData;
        }

        private HttpWebResponse PostModel(string postUrl, string userAgent, string contentType, byte[] postData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = postData.Length;

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postData, 0, postData.Length);
                requestStream.Close();
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException webex)
            {
                response = (HttpWebResponse)webex.Response;
            }

            return response;
        }
    }
}
