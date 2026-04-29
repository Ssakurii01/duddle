using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Input
{
    [CreateAssetMenu(menuName = "Unity Plugin/Arcade/Binding Asset", fileName = "ArcadeBindingAsset", order = 0)]
    public class ArcadeBindingAsset : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ArcadeButtonColor ButtonColor;
            
            [Tooltip("Shown in overlay/help. Leave empty to hide.")]
            public string Label;
        }
        
        public Entry[] Entries =  Array.Empty<Entry>();

        public string GetLabel(ArcadeButtonColor buttonColor)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].ButtonColor == buttonColor)
                {
                    return Entries[i].Label;
                }
            }
            
            return string.Empty;
        }

        public void SetLabel(ArcadeButtonColor button, string label)
        {
            label = label?.Trim() ?? string.Empty;

            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].ButtonColor == button)
                {
                    Entries[i].Label = label;
                    return;
                }
            }
            
            Array.Resize(ref Entries, Entries.Length + 1);
            Entries[^1] = new Entry()
            {
                ButtonColor = button,
                Label = label
            };
        }
    }
}
