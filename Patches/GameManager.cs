using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GlobalEnums;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

namespace Randomizer.Patches
{
	[MonoModPatch("global::GameManager")]
    public class GameManager : global::GameManager
    {

        private extern void orig_BeginScene();

        private void BeginScene()
        {
			try
			{
				SceneChanges.PatchScene(sceneName);
			}
			finally
			{
				orig_BeginScene();
			}
        }

		[MonoModReplace]
		public new IEnumerator LoadFirstScene()
        {
			yield return new WaitForEndOfFrame();
			this.entryGateName = "top1";
			this.SetState(GameState.PLAYING);
			this.ui.ConfigureMenu();
			PlayerData.instance.StartNewGame();
			this.LoadScene("Tutorial_01");
		}

		[MonoModReplace]
		public new bool LoadGame(int saveSlot)
		{
			if (saveSlot >= 0)
			{
				string saveFilename = this.GetSaveFilename(saveSlot);
				if (!string.IsNullOrEmpty(saveFilename) && File.Exists(Application.persistentDataPath + saveFilename))
				{
					try
					{
						string toDecrypt = string.Empty;
						string text = string.Empty;
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						FileStream fileStream = File.Open(Application.persistentDataPath + saveFilename, FileMode.Open);
						if (this.gameConfig.useSaveEncryption)
						{
							toDecrypt = (string)binaryFormatter.Deserialize(fileStream);
						}
						else
						{
							text = (string)binaryFormatter.Deserialize(fileStream);
						}
						fileStream.Close();
						if (this.gameConfig.useSaveEncryption)
						{
							text = StringEncrypt.DecryptData(toDecrypt);
						}
						SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(text);
						PlayerData instance = saveGameData.playerData;
						instance.itemPlacements = saveGameData.itemPlacements ?? new Serialized.SerializableStringDictionary();
						instance.obtainedLocations = saveGameData.obtainedLocations ?? new System.Collections.Generic.List<string>();
						instance.itemObtainedCounts = saveGameData.itemObtainedCounts ?? new Serialized.SerializableIntDictionary();
						instance.itemCosts = saveGameData.itemCosts ?? new Serialized.SerializableIntDictionary();
						SceneData instance2 = saveGameData.sceneData;
						PlayerData.instance = instance;
						this.playerData = instance;
						SceneData.instance = instance2;
						this.sceneData = instance2;
						this.profileID = saveSlot;
						this.inputHandler.RefreshPlayerData();
						return true;
					}
					catch (Exception ex)
					{
						Debug.LogFormat("Error loading save file for slot {0}: {1}", new object[]
						{
						saveSlot,
						ex
						});
						return false;
					}
				}
				Debug.Log("Save file not found for slot " + saveSlot);
				return false;
			}
			Debug.LogError("Save game slot not valid: " + saveSlot);
			return false;
		}
	}
}
