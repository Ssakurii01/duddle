#if NEWTONSOFT_JSON
using System;
using System.Collections.Generic;
using System.IO;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;

using Newtonsoft.Json;

using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Scripts.Missions
{
    public static class MissionsExporter
    {
#if UNITY_EDITOR
        

        private const string MissionListResourcePath = "Missions/MissionDataBase";
        private const string ExportFileName = "missions_export.json";

        [MenuItem("Luxodd Unity Plugin/Missions/Export Missions to JSON")]
        public static void ExportMissions()
        {
            var missionsList = Resources.Load<MissionDataBase>(MissionListResourcePath);
            if (missionsList == null)
            {
                LoggerHelper.LogError($"[{DateTime.Now}][{nameof(MissionsExporter)}][{nameof(ExportMissions)}] Missions data base not found at path: {MissionListResourcePath}.");
                EditorUtility.DisplayDialog("Export Error", $"Missions data base not found at path: {MissionListResourcePath}", "OK");
                return;
            }

            var export = new MissionsExport()
            {
                Missions = new List<MissionDto>()
            };

            EditorUtility.DisplayProgressBar("Exporting Missions", "", 0f);
            
            var progress = 0f;
            var totalMissions = missionsList.Missions.Count;
            var counter = 0;
            
            foreach (var mission in missionsList.Missions)
            {
                if (mission == null)
                {
                    continue;
                }

                var dto = new MissionDto()
                {
                    Id = mission.Id,
                    Name = mission.Name,
                    Description = mission.Description,
                    Type = mission.Type.ToString().ToLowerInvariant(),
                    Difficulty = mission.DifficultyLevel.ToString().ToLowerInvariant(),
                    Hardness = mission.Hardness,
                    Bet = mission.Bet,
                    Ratio = mission.Ratio,
                    Value = mission.Value,
                    Level = mission.Level,
                };
                export.Missions.Add(dto);
                progress = counter/(float)totalMissions;
                EditorUtility.DisplayProgressBar("Exporting Missions", "", progress);
            }
            
            string json = JsonConvert.SerializeObject(export, Formatting.Indented);
            string exportPath = Path.Combine(Application.dataPath, ExportFileName);
            File.WriteAllText(exportPath, json);

            LoggerHelper.Log($"[{DateTime.Now}][{nameof(MissionsExporter)}][{nameof(ExportMissions)}] OK, exported to: {exportPath}.");
            
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Export Complete", "Missions exported successfully", "OK");
            EditorUtility.RevealInFinder(exportPath);
            
        }
#endif
    }
}
#endif