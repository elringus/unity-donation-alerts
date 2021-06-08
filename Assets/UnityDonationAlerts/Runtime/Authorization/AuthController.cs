using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityDonationAlerts
{
    /// <summary>
    /// Controls authorization procedures and provides token to access the APIs.
    /// Implementation based on Google OAuth 2.0 protocol: https://developers.google.com/identity/protocols/OAuth2.
    /// </summary>
    public static class AuthController
    {
        [Serializable]
        private class SocketTokenResponse
        {
            [Serializable]
            public class Data
            {
                public string id;
                public string socket_connection_token;
            }

            public Data data = new Data();
        }

        /// <summary>
        /// Invoked when <see cref="AccessToken"/> has been refreshed.
        /// Return false on authorization fail.
        /// </summary>
        public static event Action<bool> OnAccessTokenRefreshed;

        public static string AccessToken => settings.CachedAccessToken;
        public static string SocketToken { get; private set; }
        public static string UserId { get; private set; }
        public static bool IsRefreshingAccessToken { get; private set; }

        private static DonationAlertsSettings settings;
        private static IAccessTokenProvider accessTokenProvider;

        static AuthController ()
        {
            settings = DonationAlertsSettings.LoadFromResources();

            #if UNITY_WEBGL && !UNITY_EDITOR // WebGL doesn't support loopback method; using redirection scheme instead.
            accessTokenProvider = new RedirectAccessTokenProvider(settings);
            #elif UNITY_ANDROID && !UNITY_EDITOR // On Android a native OpenID lib is used for better UX.
            accessTokenProvider = new AndroidAccessTokenProvider(settings);
            #elif UNITY_IOS && !UNITY_EDITOR // On iOS a native OpenID lib is used for better UX.
            accessTokenProvider = new IOSAccessTokenProvider(settings);
            #else // Loopback scheme is used on other platforms.
            accessTokenProvider = new LoopbackAccessTokenProvider(settings);
            #endif
        }

        public static void RefreshAccessToken ()
        {
            if (IsRefreshingAccessToken) return;
            IsRefreshingAccessToken = true;

            accessTokenProvider.OnDone += HandleAccessTokenProviderDone;
            accessTokenProvider.ProvideAccessToken();
        }

        public static void CancelAuth ()
        {
            if (IsRefreshingAccessToken)
                HandleAccessTokenProviderDone(accessTokenProvider);
        }

        private static void HandleAccessTokenProviderDone (IAccessTokenProvider provider)
        {
            accessTokenProvider.OnDone -= HandleAccessTokenProviderDone;

            var authFailed = !provider.IsDone || provider.IsError;

            if (authFailed)
            {
                Debug.LogError("UnityDonationAlerts: Failed to execute authorization procedure. Check application settings and credentials.");
                IsRefreshingAccessToken = false;
                OnAccessTokenRefreshed?.Invoke(true);
                return;
            }

            var socketTokenRequest = UnityWebRequest.Get("https://www.donationalerts.com/api/v1/user/oauth");
            socketTokenRequest.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            socketTokenRequest.SendWebRequest().completed += HandleSocketTokenProviderDone;
        }

        private static void HandleSocketTokenProviderDone (AsyncOperation op)
        {
            var request = ((UnityWebRequestAsyncOperation)op).webRequest;
            var authFailed = request.isHttpError || request.isNetworkError;

            if (authFailed) Debug.LogError("UnityDonationAlerts: Failed to execute authorization procedure. Check application settings and credentials.");
            else
            {
                var data = JsonUtility.FromJson<SocketTokenResponse>(request.downloadHandler.text).data;
                UserId = data.id;
                SocketToken = data.socket_connection_token;
            }

            IsRefreshingAccessToken = false;
            OnAccessTokenRefreshed?.Invoke(!authFailed);
        }
    }
}
