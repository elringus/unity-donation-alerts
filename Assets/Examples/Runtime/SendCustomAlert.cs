using UnityEngine;
using UnityDonationAlerts;

public class SendCustomAlert : MonoBehaviour
{
    public string ExternalId = "TestId";
    public string Header = "Test Header";
    public string Message = "Test message.";
    public int IsShown = 1;

    private void OnEnable ()
    {
        DonationAlerts.Connect();
        DonationAlerts.OnDonation += HandleDonation;
    }

    private void OnDisable ()
    {
        DonationAlerts.Disconnect();
        DonationAlerts.OnDonation -= HandleDonation;
    }

    private void HandleDonation (Donation donation)
    {
        Debug.Log($"Donation received: From: {donation.From} Message: {donation.Message} Amount: {donation.Amount} {donation.Currency}");
    }

    [ContextMenu("Send Custom Alert")]
    private void Send ()
    {
        if (DonationAlerts.ConnectionState == ConnectionState.Connected)
            DonationAlerts.SendCustomAlert(ExternalId, Header, Message, IsShown);
    }
}
