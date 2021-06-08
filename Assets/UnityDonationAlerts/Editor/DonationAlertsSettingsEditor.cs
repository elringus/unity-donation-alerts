using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityDonationAlerts
{
    [CustomEditor(typeof(DonationAlertsSettings))]
    public class DonationAlertsSettingsEditor : Editor
    {
        protected DonationAlertsSettings TargetSettings => target as DonationAlertsSettings;

        private SerializedProperty genericClientCredentials;
        private SerializedProperty accessScopes;
        private SerializedProperty loopbackUri;
        private SerializedProperty loopbackResponseHtml;
        private SerializedProperty accessTokenPrefsKey;
        private SerializedProperty refreshTokenPrefsKey;
        private SerializedProperty logDebugMessages;

        private static readonly GUIContent genericClientCredentialsContent = new GUIContent("Credentials", "DonationAlerts API application credentials used to authorize requests via loopback and redirect schemes.");
        private static readonly GUIContent accessScopesContent = new GUIContent("Access Scopes", "Scopes of access to the user's DonationAlerts the app will request.");
        private static readonly GUIContent loopbackUriContent = new GUIContent("Loopback URI", "A web address for the loopback authentication requests. Defult is 'localhost'.");
        private static readonly GUIContent loopbackResponseHtmlContent = new GUIContent("Loopback Response HTML", "HTML page shown to the user when loopback response is received.");
        private static readonly GUIContent accessTokenPrefsKeyContent = new GUIContent("Access Token Key", "PlayerPrefs key used to store access token.");
        private static readonly GUIContent refreshTokenPrefsKeyContent = new GUIContent("Refresh Token Key", "PlayerPrefs key used to store refresh token.");
        private static readonly GUIContent deleteCachedTokensContent = new GUIContent("Delete cached tokens", "Removes cached access and refresh tokens forcing user to login on the next request.");

        private static DonationAlertsSettings GetOrCreateSettings ()
        {
            var settings = DonationAlertsSettings.LoadFromResources();
            if (!settings)
            {
                settings = CreateInstance<DonationAlertsSettings>();
                Directory.CreateDirectory(Application.dataPath + "/Resources");
                const string path = "Assets/Resources/DonationAlertsSettings.asset";
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                Debug.Log($"DonationAlerts settings file didn't exist and was created at: {path}.\n" +
                          "You're free to move it, just make sure it stays in the root of a 'Resources' folder.");
            }
            return settings;
        }

        [SettingsProvider]
        internal static SettingsProvider CreateProjectSettingsProvider ()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetOrCreateSettings());
            var keywords = SettingsProvider.GetSearchKeywordsFromPath(assetPath);
            return AssetSettingsProvider.CreateProviderFromAssetPath("Project/DonationAlerts", assetPath, keywords);
        }

        private void OnEnable ()
        {
            if (!TargetSettings) return;
            genericClientCredentials = serializedObject.FindProperty("genericClientCredentials");
            accessScopes = serializedObject.FindProperty("accessScopes");
            loopbackUri = serializedObject.FindProperty("loopbackUri");
            loopbackResponseHtml = serializedObject.FindProperty("loopbackResponseHtml");
            accessTokenPrefsKey = serializedObject.FindProperty("accessTokenPrefsKey");
            refreshTokenPrefsKey = serializedObject.FindProperty("refreshTokenPrefsKey");
            logDebugMessages = serializedObject.FindProperty("logDebugMessages");
        }

        public override void OnInspectorGUI ()
        {
            if (TargetSettings.GenericClientCredentials.ContainsSensitiveData)
                EditorGUILayout.HelpBox("The asset contains sensitive data about your DonationAlerts API app. " +
                                        "Consider excluding it from the version control systems.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(genericClientCredentials, genericClientCredentialsContent, true);
            EditorGUILayout.PropertyField(accessScopes, accessScopesContent, true);
            EditorGUILayout.PropertyField(loopbackUri, loopbackUriContent);
            EditorGUILayout.PropertyField(loopbackResponseHtml, loopbackResponseHtmlContent);
            EditorGUILayout.PropertyField(accessTokenPrefsKey, accessTokenPrefsKeyContent);
            EditorGUILayout.PropertyField(refreshTokenPrefsKey, refreshTokenPrefsKeyContent);
            EditorGUILayout.PropertyField(refreshTokenPrefsKey, refreshTokenPrefsKeyContent);
            EditorGUILayout.PropertyField(logDebugMessages);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create DonationAlerts API app"))
                Application.OpenURL("https://www.donationalerts.com/application/clients");

            using (new EditorGUI.DisabledScope(!TargetSettings.IsAnyAuthTokenCached()))
                if (GUILayout.Button(deleteCachedTokensContent))
                    TargetSettings.DeleteCachedAuthTokens();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
