using System.Collections.Generic;
using UnityEngine;

namespace UnityDonationAlerts
{
    [System.Serializable]
    public class GenericClientCredentials : IClientCredentials
    {
        public string AuthUri { get => authUri; set => authUri = value; }
        public string TokenUri { get => tokenUri; set => tokenUri = value; }
        public string ClientId { get => clientId; set => clientId = value; }
        public string ClientSecret { get => clientSecret; set => clientSecret = value; }
        public List<string> RedirectUris { get => redirectUris; set => redirectUris = value; }
        public bool ContainsSensitiveData => !string.IsNullOrEmpty(ClientId + ClientSecret);

        [SerializeField] private string authUri = "https://DonationAlerts.com/api/v1.0/authorize";
        [SerializeField] private string tokenUri = "https://DonationAlerts.com/api/v1.0/token";
        [SerializeField] private string clientId = null;
        [SerializeField] private string clientSecret = null;
        [SerializeField] private List<string> redirectUris = new List<string> { "http://localhost" };
    }
}
