
namespace UnityDonationAlerts
{
    public interface IClientCredentials
    {
        string AuthUri { get; }
        string TokenUri { get; }
        string ApplicationId { get; }
        string APIKey { get; }
    }
}
