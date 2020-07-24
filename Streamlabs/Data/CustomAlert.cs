using IPA.Utilities;
using System;
using System.IO;
using UnityEngine;
using UnityStreamlabs;
using TMPro;
using BeatSaberMarkupLanguage;

namespace Streamlabs
{
    public class CustomAlert
    {
        public string FileName { get; }
        public AssetBundle AssetBundle { get; }
        public AlertDescriptor Descriptor { get; }
        public GameObject gameObject { get; }
        public StreamlabsEvent streamEvent { get; set; }

        public string ChannelPointsUser { get; set; }
        public CustomAlert(string fileName)
        {
            FileName = fileName;

            if (fileName != "DefaultNotes")
            {
                try
                {
                    AssetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(UnityGame.InstallPath, "CustomAlerts"), fileName));
                    GameObject note = AssetBundle.LoadAsset<GameObject>("assets/_customnote.prefab");

                    Descriptor = note.GetComponent<AlertDescriptor>();
                    //Descriptor.Icon = Descriptor.Icon ?? Utils.GetDefaultCustomIcon();

                    gameObject = note;
                }
                catch (Exception ex)
                {
                    /*Logger.log.Warn($"Something went wrong getting the AssetBundle for '{FileName}'!");
                    Logger.log.Warn(ex);

                    Descriptor = new NoteDescriptor
                    {
                        NoteName = "Invalid Note (Delete it!)",
                        AuthorName = FileName,
                        Icon = Utils.GetErrorIcon()
                    };

                    ErrorMessage = $"File: '{fileName}'" +
                                    "\n\nThis file failed to load." +
                                    "\n\nThis may have been caused by having duplicated files," +
                                    " another note with the same name already exists or that the custom note is simply just broken." +
                                    "\n\nThe best thing is probably just to delete it!";

                    FileName = "DefaultNotes";*/
                }
            }
            else
            {
                /*Descriptor = new NoteDescriptor
                {
                    AuthorName = "Beat Saber",
                    NoteName = "Default",
                    Description = "This is the default notes. (No preview available)",
                    Icon = Utils.GetDefaultIcon()
                };*/
            }
        }
        public CustomAlert(GameObject obj, AlertDescriptor descriptor, StreamlabsEvent labEvent)
        {
            this.Descriptor = descriptor;
            this.gameObject = obj;
            this.streamEvent = labEvent;
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
            foreach(string stringReplacement in replacementStrings)
            {

            }
            for (int i = 0; i < replacementStrings.GetLength(0); i++)
            {
                string textToReplace = replacementStrings[i, 0];
                string replaceText = replacementStrings[i, 1];
                if (original.Contains("{" + textToReplace + "}") && replaceText != null)
                {
                    replaceText = replaceText.Replace(@"<", "<\u200B").Replace(@">", "\u200B>"); // user input text is the DEVIL. i don't even know if you 
                    original = original.Replace("{" + textToReplace + "}", "<color=#32C3A6>" + replaceText+"</color>");
                }
            }
            return original;
        }
        public void Spawn()
        {
            //textObj.SetActive(true);
            try
            {
                // gameObject.InstantiatePrefab();
                Console.WriteLine("GAMER");
                //Console.WriteLine(streamEvent);
                if (gameObject == null)
                {
                    Console.WriteLine("OH NO, mesh IS FUCKY WUCKY UWU");
                    return;
                }
                GameObject spawnedObject = UnityEngine.Object.Instantiate(gameObject);
                spawnedObject.transform.parent = null;
                UnityEngine.Object.DontDestroyOnLoad(spawnedObject);
                foreach (TextMeshPro textMesh in spawnedObject.GetComponentsInChildren<TextMeshPro>())
                {
                    string[,] replacementStrings = { 
                        { "username", streamEvent.message[0].name },
                        { "amount", streamEvent.message[0].amount },
                        { "count", streamEvent.message[0].raiders.ToString() },
                        { "channelpoints", streamEvent.message[0].channelPointsName },
                        { "viewers", streamEvent.message[0].viewers.ToString() }
                    };
                    textMesh.text = ReplaceText(textMesh.text, replacementStrings);
                    //textMesh.text = textMesh.text.Replace("{username}", streamEvent.message[0].name);
                }

                // do stupid bit stuff
                if (Descriptor.AlertTriggerType == AlertDescriptor.AlertType.Bits)
                {
                    int[] bitTypes = { 100000, 10000, 5000, 1000, 100, 10 };
                    foreach(int bitType in bitTypes)
                    {
                        if (spawnedObject.transform.Find(bitType.ToString()))
                        {
                            spawnedObject.transform.Find(bitType.ToString()).gameObject.SetActive(false);
                        }
                    }
                    if (int.Parse(streamEvent.message[0].amount) < bitTypes[bitTypes.Length-1]){
                        spawnedObject.transform.Find(bitTypes[bitTypes.Length - 1].ToString()).gameObject.SetActive(true);
                    }
                    else {
                        foreach (int bitType in bitTypes)
                        {
                            if (int.Parse(streamEvent.message[0].amount) >= bitType)
                            {
                                spawnedObject.transform.Find(bitType.ToString()).gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                }
                UnityEngine.Object.Destroy(spawnedObject, Descriptor.AlertLifetime);
                //spawnedObject.e
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
