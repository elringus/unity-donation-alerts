## Description

A client for [DonationAlerts](https://www.donationalerts.com) streaming platform, allowing to send and receive events (such as donation alerts) within [Unity game engine](https://unity.com/).

## Installation

Download and import [DonationAlerts.unitypackage](https://github.com/Elringus/DonationAlerts/raw/master/DonationAlerts.unitypackage) package.

## How to use

After installing the package, go to project settings (Edit -> Project Settings) and select DonationAlerts category.

![](https://i.gyazo.com/7146364247547a91a176f03926c159e2.png) 

Click "Create DonationAlerts API app" and register a new app to get unique application ID and API key and enter them in the settings. Other fields can be left as is.

Now you can listen events in Unity, eg:

```csharp
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
    Debug.Log($"From: {donation.From} Message: {donation.Message} Amount: {donation.Amount}");
}
```

To send a test donation use "My Messages -> Add Message" (enable show in widget toggle) on DonationAlerts panel.

