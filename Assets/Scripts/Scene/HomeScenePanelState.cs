using UnityEngine;

public static class HomeScenePanelState
{
    public static bool HasSavedState { get; private set; }
    public static bool IsPreparationPanelActive { get; private set; }
    public static int SelectingSlot { get; private set; }
    public static string ReturnSceneName { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlayStart()
    {
        Clear();
        PlayerPrefs.DeleteKey("ShowPreparationPanel");
        PlayerPrefs.DeleteKey("ReturnSceneName");
        PlayerPrefs.DeleteKey("CurrentSelectingSlot");
        PlayerPrefs.DeleteKey("TempSelectedCharacterId");
        PlayerPrefs.DeleteKey("TempSelectedCharacterName");
        PlayerPrefs.DeleteKey("TempSelectedCharacterIconId");
        PlayerPrefs.DeleteKey("PreparingStageIndex");
        PlayerPrefs.Save();
    }

    public static void Save(bool isPreparationPanelActive, int selectingSlot, string returnSceneName)
    {
        HasSavedState = true;
        IsPreparationPanelActive = isPreparationPanelActive;
        SelectingSlot = selectingSlot;
        ReturnSceneName = returnSceneName;
    }

    public static void Clear()
    {
        HasSavedState = false;
        IsPreparationPanelActive = false;
        SelectingSlot = 0;
        ReturnSceneName = string.Empty;
    }
}
