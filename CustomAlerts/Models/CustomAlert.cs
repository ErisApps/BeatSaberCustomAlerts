using System;
using System.IO;
using BeatSaberMarkupLanguage;
using CustomAlerts.Models.Events;
using IPA.Utilities;
using TMPro;
using UnityEngine;

namespace CustomAlerts.Models
{
    public class CustomAlert : IAlert
    {
        public string FileName { get; }
        public GameObject GameObject { get; }
        public AssetBundle AssetBundle { get; }
        public AlertDescriptor Descriptor { get; }
        public string ChannelPointsUser { get; set; }
        public TwitchEvent TwitchEvent { get; set; }
        public AlertType AlertType => TwitchEvent?.AlertType ?? Descriptor.alertTriggerType;
        public float Lifeline => Descriptor.alertLifetime;
        public float Flatline { get; set; } = 0f;
        public int Volume { get; set; } = 100;

        private Sprite _sprite;
        public Sprite Sprite
        {
            get
            {
                if (_sprite == null && Descriptor != null && Descriptor.icon != null)
                {
                    _sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteFromTexture(Descriptor.icon);
                }
                else if (_sprite == null)
                {
                    _sprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
                }
                return _sprite;
            }
        }

        public CustomAlert(string fileName)
        {
            FileName = fileName;

            if (FileName != "No Alerts")
            {
                try
                {
                    AssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(UnityGame.UserDataPath, "CustomAlerts"), FileName));
                    GameObject alert = AssetBundle.LoadAsset<GameObject>("Assets/_CustomAlert.prefab");
                    Descriptor = alert.GetComponent<AlertDescriptor>();
                    GameObject = alert;

                    
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"\nUnable to load the AssetBundle {FileName}: {e.Message}");
                }
            }
            else
            {
                // eh?
            }
        }

        public CustomAlert(GameObject @object, AlertDescriptor descriptor, TwitchEvent twitchEvent)
        {
            GameObject = @object;
            Descriptor = descriptor;
            TwitchEvent = twitchEvent;
        }

        public static TMP_Text CreateText(RectTransform rectTransform, Vector3 anchoredPosition)
        {
            TMP_Text tmp_text = BeatSaberUI.CreateText(rectTransform, "", anchoredPosition);
            tmp_text.alignment = TextAlignmentOptions.Center;
            tmp_text.fontSize = 4f;
            tmp_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 2f);
            tmp_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2f);
            tmp_text.enableWordWrapping = false;
            tmp_text.overflowMode = TextOverflowModes.Overflow;
            return tmp_text;
        }

        public static string ReplaceText(string original, string[,] replacementStrings)
        {
            for (int i = 0; i < replacementStrings.GetLength(0); i++)
            {
                string textToReplace = replacementStrings[i, 0];
                string replaceText = replacementStrings[i, 1];
                if (original.Contains("{" + textToReplace + "}") && replaceText != null)
                {
                    replaceText = replaceText.Replace(@"<", "<\u200B").Replace(@">", "\u200B>"); // user input text is the DEVIL. i don't even know if you 
                    original = original.Replace("{" + textToReplace + "}", "<color=#32C3A6>" + replaceText + "</color>"); // IF I WHAT?!
                }
            }
            return original;
        }

        public void Spawn()
        {
            try
            {
                if (GameObject == null)
                {
                    Plugin.Log.Warn("CustomAlert game object is NOT present.. not spawning");
                    return;
                }
                GameObject spawned = UnityEngine.Object.Instantiate(GameObject, null, true);
                // apply volume before the audiosources activate
                foreach (AudioSource audioSource in spawned.GetComponentsInChildren<AudioSource>())
                {
                    float adjustedVolume = audioSource.volume * (Volume / 100f);
                    audioSource.volume = adjustedVolume <= 1 ? adjustedVolume : 1;
                }

                UnityEngine.Object.DontDestroyOnLoad(spawned);
                if (TwitchEvent != null)
                {
                    foreach (TextMeshPro textMesh in spawned.GetComponentsInChildren<TextMeshPro>())
                    {
                        string[,] replacementStrings = {
                            { "username", TwitchEvent.Message.Name },
                            { "amount", TwitchEvent.Message.Amount },
                            { "count", TwitchEvent.Message.Raiders.ToString() },
                            { "channelpoints", TwitchEvent.Message.ChannelPointsName },
                            { "viewers", TwitchEvent.Message.Viewers.ToString() }
                        };
                        textMesh.text = ReplaceText(textMesh.text, replacementStrings);
                    }
                }
                if (Descriptor.alertTriggerType == AlertType.Bits)
                {
                    int[] bitTypes = { 100000, 10000, 5000, 1000, 100, 10 };
                    foreach (int bitType in bitTypes)
                    {
                        if (spawned.transform.Find(bitType.ToString()))
                        {
                            spawned.transform.Find(bitType.ToString()).gameObject.SetActive(false);
                        }
                    }
                    if (TwitchEvent != null)
                    {
                        if (int.Parse(TwitchEvent.Message.Amount) < bitTypes[bitTypes.Length - 1])
                        {
                            spawned.transform.Find(bitTypes[bitTypes.Length - 1].ToString()).gameObject.SetActive(true);
                        }
                        else
                        {
                            foreach (int bitType in bitTypes)
                            {
                                if (int.Parse(TwitchEvent.Message.Amount) >= bitType)
                                {
                                    spawned.transform.Find(bitType.ToString()).gameObject.SetActive(true);
                                    break;
                                }
                            }
                        }
                    }
                }
                UnityEngine.Object.Destroy(spawned, Descriptor.alertLifetime);
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Error while trying to spawn Custom Alert: {e.Message}");
            }
        }

        public void Destroy()
        {
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
            }
            else
            {
                UnityEngine.Object.Destroy(Descriptor);
            }
        }
    }
}