﻿using System.Collections.Generic;
using UnityEngine;

namespace UnityDonationAlerts
{
    /// <summary>
    /// Project-specific DonationAlerts settings resource.
    /// </summary>
    public class DonationAlertsSettings : ScriptableObject
    {
        public const string RequestContentType = "application/x-www-form-urlencoded";
        public const string CodeChallengeMethod = "S256";
        public const int UnauthorizedResponseCode = 401;

        /// <summary>
        /// DonationAlerts API application credentials used to authorize requests via loopback and redirect schemes.
        /// </summary>
        public GenericClientCredentials GenericClientCredentials { get => genericClientCredentials; set => genericClientCredentials = value; }
        /// <summary>
        /// Scopes of access to the user's DonationAlerts the app will request.
        /// </summary>
        public List<string> AccessScopes { get => accessScopes; set => accessScopes = value; }
        /// <summary>
        /// Joined version of the <see cref="AccessScopes"/>.
        /// </summary>
        public string AccessScope => string.Join("+", AccessScopes.ToArray());
        /// <summary>
        /// A web address for the loopback authentication requests.
        /// </summary>
        /// <see href="https://forum.unity.com/threads/515360/page-2#post-3504547"/>
        public string LoopbackUri { get => loopbackUri; set => loopbackUri = value; }
        /// <summary>
        /// HTML page shown to the user when loopback response is received.
        /// </summary>
        public string LoopbackResponseHtml { get => loopbackResponseHtml; set => loopbackResponseHtml = value; }
        /// <summary>
        /// Token used to authenticate requests; cached in <see cref="PlayerPrefs"/>.
        /// </summary>
        public string CachedAccessToken { get => PlayerPrefs.GetString(accessTokenPrefsKey); set => PlayerPrefs.SetString(accessTokenPrefsKey, value); }
        /// <summary>
        /// Token used to refresh access tokens; cached in <see cref="PlayerPrefs"/>.
        /// </summary>
        public string CachedRefreshToken { get => PlayerPrefs.GetString(refreshTokenPrefsKey); set => PlayerPrefs.SetString(refreshTokenPrefsKey, value); }
        /// <summary>
        /// Whether to log debug messages.
        /// </summary>
        public bool LogDebugMessages { get => logDebugMessages; set => logDebugMessages = value; }

        [SerializeField] private GenericClientCredentials genericClientCredentials;
        [SerializeField] private List<string> accessScopes = new List<string> {
            "oauth-user-show", "oauth-donation-subscribe", "oauth-donation-index",
            "oauth-custom_alert-store", "oauth-goal-subscribe", "oauth-poll-subscribe"
        };
        [SerializeField] private string loopbackUri = "http://localhost";
        [SerializeField] private string loopbackResponseHtml = "<html><h1>Please return to the app.</h1></html>";
        [SerializeField] private string accessTokenPrefsKey = "DonationAlertsAccessToken";
        [SerializeField] private string refreshTokenPrefsKey = "DonationAlertsRefreshToken";
        [SerializeField] private bool logDebugMessages = false;

        /// <summary>
        /// Retrieves settings from the project resources.
        /// </summary>
        public static DonationAlertsSettings LoadFromResources () => Resources.Load<DonationAlertsSettings>(nameof(DonationAlertsSettings));

        /// <summary>
        /// Removes cached access and refresh tokens forcing user to login on the next request.
        /// </summary>
        public void DeleteCachedAuthTokens ()
        {
            if (PlayerPrefs.HasKey(accessTokenPrefsKey))
                PlayerPrefs.DeleteKey(accessTokenPrefsKey);
            if (PlayerPrefs.HasKey(refreshTokenPrefsKey))
                PlayerPrefs.DeleteKey(refreshTokenPrefsKey);
        }

        /// <summary>
        /// Whether access or refresh tokens are currently cached in <see cref="PlayerPrefs"/>.
        /// </summary>
        public bool IsAnyAuthTokenCached ()
        {
            return PlayerPrefs.HasKey(accessTokenPrefsKey) || PlayerPrefs.HasKey(refreshTokenPrefsKey);
        }
    }
}
