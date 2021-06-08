using System.Collections.Generic;
using UnityEngine;

namespace UnityDonationAlerts
{
    [System.Serializable]
    public class GenericClientCredentials : IClientCredentials
    {
        public string AuthUri { get => authUri; set => authUri = value; }
        public string TokenUri { get => tokenUri; set => tokenUri = value; }
        public string ApplicationId { get => applicationId; set => applicationId = value; }
        public string APIKey { get => apiKey; set => apiKey = value; }
        public List<string> RedirectUris { get => redirectUris; set => redirectUris = value; }
        public bool ContainsSensitiveData => !string.IsNullOrEmpty(ApplicationId + APIKey);

        [SerializeField] private string authUri = "https://www.donationalerts.com/oauth/authorize";
        [SerializeField] private string tokenUri = "https://www.donationalerts.com/oauth/token";
        [SerializeField] private string applicationId = null;
        [SerializeField] private string apiKey = null;
        [SerializeField] private List<string> redirectUris = new List<string> { "http://localhost" };
    }
}
