using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

namespace UnityDonationAlerts
{
    public static class DonationAlerts
    {
        #pragma warning disable CS0649
        [Serializable]
        private struct ClientIdResponse
        {
            [Serializable]
            public struct Result
            {
                public string client;
            }

            public Result result;
        }

        [Serializable]
        private struct ChannelResponse
        {
            [Serializable]
            public struct Channel
            {
                public string token;
            }

            public List<Channel> channels;
        }

        [Serializable]
        private struct DonationMessage
        {
            [Serializable]
            public struct Result
            {
                public Data data;
            }

            [Serializable]
            public struct Data
            {
                public DonationData data;
            }

            [Serializable]
            public struct DonationData
            {
                public string username;
                public string message;
                public double amount;
                public string currency;
            }

            public Result result;
            public DonationData Donation => result.data.data;
        }
        #pragma warning restore CS0649

        /// <summary>
        /// Invoked when <see cref="ConnectionState"/> is changed.
        /// </summary>
        public static event Action<ConnectionState> OnConnectionStateChanged;
        /// <summary>
        /// Invoked when a donation is sent.
        /// </summary>
        public static event Action<Donation> OnDonation;

        /// <summary>
        /// Current connection state to the DonationAlerts server.
        /// </summary>
        public static ConnectionState ConnectionState { get; private set; }

        private static SynchronizationContext unitySyncContext;
        private static UnityWebRequest request;
        private static WebSocket webSocket;
        private static DonationAlertsSettings settings;

        public static void Connect ()
        {
            if (ConnectionState == ConnectionState.Connected || ConnectionState == ConnectionState.Connecting) return;

            settings = DonationAlertsSettings.LoadFromResources();
            unitySyncContext = SynchronizationContext.Current;

            ChangeConnectionState(ConnectionState.Connecting);

            AuthController.OnAccessTokenRefreshed += HandleAccessTokenRefreshed;
            AuthController.RefreshAccessToken();

            void HandleAccessTokenRefreshed (bool success)
            {
                AuthController.OnAccessTokenRefreshed -= HandleAccessTokenRefreshed;
                InitializeWebSocket();
            }
        }

        public static void Disconnect ()
        {
            if (request != null)
            {
                request.Abort();
                request.Dispose();
                request = null;
            }

            webSocket?.Close();

            ChangeConnectionState(ConnectionState.NotConnected);
        }

        public static UnityWebRequestAsyncOperation SendCustomAlert (string externalId, string header, string message, int isShown)
        {
            if (request != null)
            {
                Debug.LogError("Can't send custom alert: send request already in progress.");
                return null;
            }

            if (ConnectionState != ConnectionState.Connected)
            {
                Debug.LogError("Can't send custom alert: not connected.");
                return null;
            }

            var form = new WWWForm();
            form.AddField("external_id", externalId);
            form.AddField("header", header);
            form.AddField("message", message);
            form.AddField("is_shown", isShown.ToString(CultureInfo.InvariantCulture));

            request = UnityWebRequest.Post("https://www.donationalerts.com/api/v1/custom_alert", form);
            request.SetRequestHeader("Content-Type", DonationAlertsSettings.RequestContentType);
            request.SetRequestHeader("Accept", "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.SetRequestHeader("Authorization", $"Bearer {AuthController.AccessToken}");

            var sendOperation = request.SendWebRequest();
            sendOperation.completed += HandleResponse;

            return sendOperation;

            void HandleResponse (AsyncOperation op)
            {
                if (request.isHttpError || request.isNetworkError)
                    Debug.LogError($"UnityDonationAlerts: Failed to send custom alert: {request.error}");
                Debug.Log(request.downloadHandler.text);
                request.Dispose();
                request = null;
            }
        }

        private static void InitializeWebSocket ()
        {
            webSocket = new WebSocket("wss://centrifugo.donationalerts.com/connection/websocket");
            webSocket.EmitOnPing = true;
            webSocket.OnOpen += HandleOpen;
            webSocket.OnClose += HandleClose;
            webSocket.OnError += HandleError;
            webSocket.OnMessage += HandleSocketMessage;
            webSocket.Connect();
            webSocket.Send($"{{\"params\":{{\"token\":\"{AuthController.SocketToken}\"}},\"id\":1}}");

            void HandleOpen (object sender, EventArgs evt)
            {
                if (settings.LogDebugMessages)
                    Debug.Log("WebSocket connection opened.");
                ChangeConnectionState(ConnectionState.Connected);
            }

            void HandleClose (object sender, CloseEventArgs evt)
            {
                if (settings.LogDebugMessages)
                    Debug.Log($"WebSocket connection closed. {evt.Code} {evt.Reason}");
                ChangeConnectionState(ConnectionState.NotConnected);
            }

            void HandleError (object sender, ErrorEventArgs evt)
            {
                Debug.LogError($"DonationAlerts web socket error: {evt.Exception} {evt.Message}");
            }
        }

        private static void HandleSocketMessage (object sender, MessageEventArgs evt)
        {
            try
            {
                if (settings.LogDebugMessages)
                    Debug.Log("WebSocket: " + evt.Data);

                if (evt.Data.StartsWith("{\"id\":1")) // part of the auth
                    unitySyncContext.Send(SubscribeToChannels, JsonUtility.FromJson<ClientIdResponse>(evt.Data).result.client);
                else if (evt.Data.StartsWith("{\"result\":{\"channel\":\"$alerts:donation")) // donation
                {
                    var data = JsonUtility.FromJson<DonationMessage>(evt.Data).Donation;
                    var donation = new Donation { From = data.username, Amount = data.amount, Currency = data.currency, Message = data.message };
                    unitySyncContext.Send(SafeInokeDonation, donation);
                }
            }
            catch (Exception e) { Debug.LogWarning($"WebSocket handle message fail: {e.Message}"); }

            void SafeInokeDonation (object donation) => OnDonation?.Invoke(donation as Donation);
        }

        private static void SubscribeToChannels (object clientId)
        {
            var request = new UnityWebRequest("https://www.donationalerts.com/api/v1/centrifuge/subscribe", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes($"{{\"channels\":[\"$alerts:donation_{AuthController.UserId}\"],\"client\":\"{clientId}\"}}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {AuthController.AccessToken}");
            request.SendWebRequest().completed += HandleResponse;

            void HandleResponse (AsyncOperation op)
            {
                var token = JsonUtility.FromJson<ChannelResponse>(request.downloadHandler.text).channels[0].token;
                webSocket.Send($"{{\"params\":{{\"channel\":\"$alerts:donation_{AuthController.UserId}\",\"token\":\"{token}\"}},\"method\":1,\"id\":2}}");
            }
        }

        private static void ChangeConnectionState (ConnectionState state)
        {
            if (ConnectionState == state) return;

            ConnectionState = state;

            // This method is called on a background thread; rerouting it to the Unity's thread.
            unitySyncContext.Send(SafeInoke, state);
            void SafeInoke (object obj) => OnConnectionStateChanged?.Invoke((ConnectionState)obj);
        }
    }
}
