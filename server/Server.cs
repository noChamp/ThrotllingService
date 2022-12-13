using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ermetic.Server
{
    internal interface IServer
    {
        void Start();
        void Stop();
    }

    internal class Server : IServer
    {
        private HttpListener _listener;

        private const string URL = "http://localhost:8080/";
        private const string CLIENT_ID = "clientId";
        private const int TIME_FRAME_IN_SECONDS = 5;
        private const int ALLOWED_REQUESTS_IN_TIME_FRAME = 5;

        private class ClientThresholdInfo
        {
            public DateTime DateTime { get; set; }
            public int Count { get; set; }
        }

        private ConcurrentDictionary<int, ClientThresholdInfo> _timeFrames = new ConcurrentDictionary<int, ClientThresholdInfo>();

        private Task HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            return Task.Run(() =>
            {
                string? clientId = request.QueryString[CLIENT_ID];

                if(string.IsNullOrWhiteSpace(clientId))
                {
                    return;
                }

                int id = 0;
                bool success = int.TryParse(clientId, out id);
                
                if(!success)
                {
                    return;
                }

                if(request.HttpMethod != "GET")
                {
                    return;
                }

                DecideResponse(id, response);
            });
        }

        private void DecideResponse(int id, HttpListenerResponse response)
        {
            if (_timeFrames.ContainsKey(id))
            {
                bool isInTimeFrame = false;

                if (_timeFrames[id].DateTime > DateTime.Now)
                {
                    isInTimeFrame = _timeFrames[id].DateTime - DateTime.Now < TimeSpan.FromSeconds(TIME_FRAME_IN_SECONDS);
                }
                else
                {
                    isInTimeFrame = false;
                }

                if (isInTimeFrame)
                {
                    bool isCountOk = _timeFrames[id].Count < ALLOWED_REQUESTS_IN_TIME_FRAME;

                    if (isCountOk)
                    {
                        _timeFrames[id].Count++;
                        SendOkResponse(response);
                    }
                    else
                    {
                        SendServiceUnavailableResponse(response);
                    }
                }
                else
                {
                    _timeFrames[id].DateTime = DateTime.Now + TimeSpan.FromSeconds(TIME_FRAME_IN_SECONDS);
                    _timeFrames[id].Count = 1;
                    SendOkResponse(response);
                }
            }
            else
            {
                _timeFrames[id] = new ClientThresholdInfo()
                {
                    DateTime = DateTime.Now + TimeSpan.FromSeconds(TIME_FRAME_IN_SECONDS),
                    Count = 1
                };

                SendOkResponse(response);
            }
        }

        private void SendOkResponse(HttpListenerResponse response)
        {
            SendResponse(response, 200);
        }

        private void SendServiceUnavailableResponse(HttpListenerResponse response)
        {
            SendResponse(response, 503);
        }

        private void SendResponse(HttpListenerResponse response, int statusCode)
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = statusCode;

            response.Close();
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                try
                {
                    var context = _listener.EndGetContext(result);

                    HttpListenerRequest req = context.Request;
                    HttpListenerResponse resp = context.Response;

                    HandleRequest(req, resp);

                    _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);

                }
                // the code might fall in _listener.EndGetContext() or _listener.BeginGetContext()
                // in case the cpu passed the if (_listener.IsListening) and the user called Stop()
                catch (Exception)
                {
                    return;
                }
            }
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(URL);
            _listener.Start();

            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
