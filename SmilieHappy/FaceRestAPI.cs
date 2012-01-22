// This file is modified from the original by Stuart Lodge at Cirrious
// Especially, some async http code has been hacked in
// But really... this code could do with a bit of an overhaul.

/*!
 * Face.com Rest API C# Library v1.0.1
 * http://face.com/
 *
 * Copyright 2010, 
 * Written By Daren Willman
 *  
 * Date: Friday July 10
 * 
 * Note: XML Serialization not currently supported.
 *
 * v1.0.1 - add facebook auth to constrcutor

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

public class FaceRestAPI
{
    private string apiKey;
    private string apiSecret;
    private string password = null;
    private bool getRawData;
    private string format;
    private string api_server = "http://api.face.com/";

    private Dictionary<string, string> userAuth = new Dictionary<string, string>();

    public FaceRestAPI(string apiKey, string apiSecret, string password, bool getRawData, string format, string fb_user, string fb_oauth_token)
    {
        this.apiKey = apiKey;
        this.apiSecret = apiSecret;
        this.password = password;
        this.getRawData = getRawData;

        userAuth.Add("fb_user", fb_user);
        userAuth.Add("fb_oauth_token", fb_oauth_token);

        if (!getRawData)
        {
            this.format = "json";
        }
        else
        {
            this.format = format;
        }
    }

    // *************
    // Account Methods
    // *************
    public void account_authenticate(
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        this.call_method("account/authenticate", d, success, error);
    }

    public void account_users(List<string> ns,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(ns);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("namespaces", list[0]);
        this.call_method("account/users", dict, success, error);
    }

    public void account_limits(Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        this.call_method("account/limits", d, success, error);
    }


    // *************
    // Face Methods
    // *************
    public void faces_detect(
        List<string> urls, 
        string filename, 
        List<string> ownerIds, 
        string callBackUrl,
        Action<FaceRestAPI.FaceAPI> success,
        Action<Exception> error)
    {
        List<string> list = this.prep_lists(urls, ownerIds);
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("urls", list[0]);
        dict.Add("owner_ids", list[1]);
        dict.Add("_file", "@" + filename);
        dict.Add("callback_url", callBackUrl);

        this.call_method("faces/detect", dict, success, error);
    }

    public void faces_train(List<string> uids, string ns, string callbackUrl,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(uids);
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("uids", list[0]);
        dict.Add("namespace", ns);
        dict.Add("callback_url", callbackUrl);

        this.call_method("faces/train", dict, success, error);
    }

    public void faces_recognize(List<string> urls, List<string> uids, string ns, string train, string filename, List<string> ownerIds, string callbackUrl, Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(urls, uids, ownerIds);
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("urls", list[0]);
        dict.Add("uids", list[1]);
        dict.Add("namespace", ns);
        dict.Add("train", train);
        dict.Add("owner_ids", list[2]);
        dict.Add("_file", "@" + filename);
        dict.Add("callback_url", callbackUrl);

        call_method("faces/recognize", dict, success, error);

    }

    public void faces_status(List<string> uids, string ns,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(uids);
        var dict = new Dictionary<string, string> {{"uids", list[0]}, {"namespace", ns}};

        this.call_method("faces/status", dict, success, error);
    }

    // ************
    // Tags Methods
    // ************
    public void tags_add(string url, string x, string y, string width, string height, string label, string uid, string pid, string taggerId, string ownerId,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        var dict = new Dictionary<string, string>
                       {
                           {"url", url},
                           {"x", x},
                           {"y", y},
                           {"width", width},
                           {"height", height},
                           {"label", label},
                           {"uid", uid},
                           {"pid", pid},
                           {"tagger_id", taggerId},
                           {"owner_id", ownerId}
                       };
        this.call_method("tags/add", dict, success, error);
    }

    public void tags_save(List<string> tids, string uid, string label, string taggerId,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(tids);
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("tids", list[0]);
        dict.Add("label", label);
        dict.Add("uid", uid);
        dict.Add("tagger_id", taggerId);

        this.call_method("tags/save", dict, success, error);
    }

    public void tags_remove(List<string> tids, string taggerId,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(tids);
        Dictionary<string, string> dict = new Dictionary<string, string>();

        dict.Add("tids", list[0]);
        dict.Add("tagger_id", taggerId);

        this.call_method("tags/remove", dict, success, error);
    }

    public void tags_get(List<string> urls, List<string> pids, string filename, List<string> ownerIds, List<string> uids, string ns, string filter, string limit, string together, string order
        , Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(urls, pids, ownerIds, uids);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("urls", list[0]);
        dict.Add("pids", list[1]);
        dict.Add("owner_ids", list[2]);
        dict.Add("_file", filename);
        dict.Add("uids", list[3]);
        dict.Add("together", together);
        dict.Add("filter", filter);
        dict.Add("order", order);
        dict.Add("limit", limit);
        dict.Add("namespace", ns);

        this.call_method("tags/get", dict, success, error);
    }

    // ************
    // Facebook Methods
    // ************
    public void facebook_get(
        List<string> uids,
        string filter,
        string limit,
        string together,
        string order,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        List<string> list = this.prep_lists(uids);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("uids", list[0]);
        dict.Add("limit", limit);
        dict.Add("together", together);
        dict.Add("filter", filter);
        dict.Add("order", order);

        this.call_method("facebook/get", dict, success, error);
    }


    protected void call_method(string method,
        Dictionary<string, string> param,
        Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        // Remember keys for removal
        List<string> keys = new List<string>();

        foreach (KeyValuePair<string, string> s in param)
        {
            if (String.IsNullOrWhiteSpace(s.Value))
            {
                keys.Add(s.Key);
            }
            else
            {
                if (s.Key == "_file" && s.Value == "@")
                {
                    keys.Add(s.Key);
                }
            }
        }
        foreach (string s in keys)
        {
            param.Remove(s);
        }


        Dictionary<string, string> authParams = new Dictionary<string, string>();

        if (!String.IsNullOrWhiteSpace(this.apiKey))
        {
            authParams.Add("api_key", this.apiKey);
        }

        if (!String.IsNullOrWhiteSpace(this.apiSecret))
        {
            authParams.Add("api_secret", this.apiSecret);
        }

        if (userAuth.Count > 0)
        {
            authParams.Add("user_auth", getUserAuthString(this.userAuth));
        }

        if (!String.IsNullOrWhiteSpace(this.password))
        {
            authParams.Add("password", this.password);
        }


        Dictionary<String, String> paramMerge = new Dictionary<String, String>();
        paramMerge = authParams.Union(param).ToDictionary(pair => pair.Key, pair => pair.Value);

        string request = method + "." + this.format;

        this.post_method(request, paramMerge, success, error);
    }

    private string http_build_query(Dictionary<string, string> param)
    {
        string strRetVal = "";

        if (param.Count > 0)
        {
            foreach (KeyValuePair<string, string> s in param)
            {
                strRetVal += s.Key + "=" + s.Value + "&";
            }
            strRetVal = strRetVal.Substring(0, strRetVal.Length - 1);
        }

        return strRetVal;
    }

    private string getUserAuthString(Dictionary<string, string> userAuthReturn)
    {
        string strRetVal = "";

        if (userAuthReturn.Count > 0)
        {
            foreach (KeyValuePair<string, string> s in userAuthReturn)
            {
                strRetVal += s.Key + ":" + s.Value + ",";
            }
            strRetVal = strRetVal.Substring(0, strRetVal.Length - 1);
        }

        return strRetVal;
    }

    private void post_method(
        string requestPath,
        Dictionary<string, string> param,
        Action<FaceRestAPI.FaceAPI> success,
        Action<Exception> error)
    {
        string url = this.api_server + requestPath;
        string paramQS = http_build_query(param);

        var request = (HttpWebRequest)WebRequest.Create(url);

        request.Method = "POST";

        Action<Stream> requestStreamWriter;

        string filename;
        if (param.TryGetValue("_file", out filename))
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            requestStreamWriter = GenerateRequestStreamWriterForFile(param, filename, boundary);
        }
        else
        {
            request.ContentType = "application/x-www-form-urlencoded";
#warning Do I need to set this ContentLength header myself?
            //req.ContentLength = paramQS.Length;
            requestStreamWriter = (requestStream) => requestStream.Write(Encoding.UTF8.GetBytes(paramQS), 0, paramQS.Length);
        }

        request.BeginGetRequestStream((getRequestAsyncResult) => 
                                        ProcessRequest(
                                        request,
                                        getRequestAsyncResult,
                                        requestStreamWriter,
                                        success,
                                        error), null);
    }

    private static Action<Stream> GenerateRequestStreamWriterForFile(Dictionary<string, string> param, string filename, string boundary)
    {
        Stream memStream = new System.IO.MemoryStream();
        byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

        string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

        foreach (KeyValuePair<string, string> s in param)
        {
            string formitem = string.Format(formdataTemplate, s.Key, s.Value);
            byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
            memStream.Write(formitembytes, 0, formitembytes.Length);
        }

        memStream.Write(boundarybytes, 0, boundarybytes.Length);

        string headerTemplate =
            "Content-Disposition: form-data; name=\"_file\"; filename=\"_files\"\r\n Content-Type: application/octet-stream\r\n\r\n";

        string header = string.Format(headerTemplate, "file1", filename);
        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
        memStream.Write(headerbytes, 0, headerbytes.Length);
        ReadFileIntoMemoryStream(memStream, filename);
        memStream.Write(boundarybytes, 0, boundarybytes.Length);

#warning Do I need to set this ContentLength header myself?
        //req.ContentLength = memStream.Length;
        Action<Stream> requestStreamWriter = (requestStream) =>
                                         {
                                             memStream.Position = 0;
                                             byte[] tempBuffer = new byte[memStream.Length];
                                             memStream.Read(tempBuffer, 0, tempBuffer.Length);
                                             memStream.Close();
                                             requestStream.Write(tempBuffer, 0,
                                                                 tempBuffer.Length);
                                         };
        return requestStreamWriter;
    }

    private static void ReadFileIntoMemoryStream(Stream memStream, string filename)
    {
        using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
        {
            using (var fileStream = iso.OpenFile(filename.Replace("@", ""), FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];

                int bytesRead = 0;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memStream.Write(buffer, 0, bytesRead);
                }                
            }
        }

        //using (var fileStream = new FileStream(filename.Replace("@", ""), FileMode.Open, FileAccess.Read))
        //{
        //    byte[] buffer = new byte[1024];

        //    int bytesRead = 0;

        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
        //    {
        //        memStream.Write(buffer, 0, bytesRead);
        //    }
        //}
    }

    private void ProcessRequest(WebRequest request,
                                IAsyncResult requestAsyncResult,
                                Action<Stream> requestStreamWriter,
                                Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        try
        {
            using (var requestStream = request.EndGetRequestStream(requestAsyncResult))
            {
                requestStreamWriter(requestStream);
            }

            request.BeginGetResponse((responseAsyncResult) => ProcessResponse(request, responseAsyncResult, success, error)
                                 , null);
        }
        catch (ThreadAbortException)
        {
            throw;
        }
        catch (Exception exception)
        {
            error(exception);
        }
    }

    private void ProcessResponse(WebRequest request,
                                IAsyncResult responseAsyncResult,
                                Action<FaceRestAPI.FaceAPI> success,
                                Action<Exception> error)
    {
        try
        {
            using (var response = request.EndGetResponse(responseAsyncResult))
            {
                string result;
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                success(Parse(result));
            }
        }
        catch (ThreadAbortException)
        {
            throw;
        }
        catch (Exception exception)
        {
            error(exception);
            return;
        }

    }

    private FaceRestAPI.FaceAPI Parse(string text)
    {
        FaceRestAPI.FaceAPI fd = null;
        if (this.format == "json")
        {
            fd = Newtonsoft.Json.JsonConvert.DeserializeObject<FaceRestAPI.FaceAPI>(text);
            fd.rawData = text;
        }
        if (this.format == "xml")
        {
            //fd = DeserializeObject<FaceRestAPI.FaceAPI>(result);
            fd = new FaceRestAPI.FaceAPI { rawData = text };
        }
        return fd;
    }

    protected List<string> prep_lists(params object[] obj)
    {
        List<string> list = new List<string>();

        foreach (object o in obj)
        {
            List<string> l = (List<string>)o;

            string str = "";
            if (l != null)
            {
                foreach (string s in l)
                {
                    str += s + ",";
                }
                if (str.Length > 0)
                {
                    str = str.Substring(0, str.Length - 1);
                }
                list.Add(str);
            }
            else
            {
                list.Add(str);
            }
        }
        return list;
    }


    private static string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters, 0, characters.Count());
        return (constructedString);
    }

    // Face API Classes
    public class FaceAPI
    {
        public List<Photo> photos { get; set; }

        public List<UserStatus> user_statuses { get; set; }

        public string status { get; set; }

        public string authenticated { get; set; }

        public Usage usage { get; set; }

        public string error_code { get; set; }

        public string error_message { get; set; }

        public Dictionary<string, List<string>> users { get; set; }

        public string rawData { get; set; }
    }

    public class Usage
    {
        public string used { get; set; }

        public string remaining { get; set; }

        public string limit { get; set; }

        public string reset_time_text { get; set; }

        public string reset_time { get; set; }

        public string namespace_used { get; set; }

        public string namespace_remaining { get; set; }

        public string namespace_limit { get; set; }
    }

    public class Photo
    {
        public string url { get; set; }

        public string pid { get; set; }

        public string width { get; set; }

        public string height { get; set; }

        public List<Tag> tags { get; set; }

        public string error_code { get; set; }

        public string error_message { get; set; }
    }

    public class Tag
    {
        public string tid { get; set; }

        public string threshold { get; set; }

        public List<UID> uids { get; set; }

        public string gid { get; set; }

        public string label { get; set; }

        public bool confirmed { get; set; }

        public bool manual { get; set; }

        public string tagger_id { get; set; }

        public float width { get; set; }

        public float height { get; set; }

        public Point center { get; set; }

        public Point eye_left { get; set; }

        public Point eye_right { get; set; }

        public Point mouth_left { get; set; }

        public Point mouth_right { get; set; }

        public Point mouth_center { get; set; }

        public Point nose { get; set; }

        public Point ear_left { get; set; }

        public Point ear_right { get; set; }

        public Point chin { get; set; }

        public string yaw { get; set; }

        public string roll { get; set; }

        public string pitch { get; set; }

        public Dictionary<string, Confidence> attributes { get; set; }

    }

    public class Point
    {
        public float x { get; set; }

        public float y { get; set; }
    }

    public class Confidence
    {
        public string value { get; set; }

        public float confidence { get; set; }
    }

    public class UID
    {
        public string uid { get; set; }

        public float confidence { get; set; }
    }

    public class Attributes
    {
        public Confidence face { get; set; }

        public Confidence gender { get; set; }

        public Confidence glasses { get; set; }

        public Confidence smiling { get; set; }
    }

    public class UserStatus
    {
        public string uid { get; set; }

        public string training_set_size { get; set; }

        public string last_trained { get; set; }

        public string training_in_progress { get; set; }
    }
}

