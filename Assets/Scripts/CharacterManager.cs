using UnityEngine;

[System.Serializable]
public class WeaponInfo
{
    public string weaponName;
    public int weaponLevel = 1;
    public int upgradeCost = 100; // レベルアップに必要な金額
    public Sprite weaponIcon;
}

[System.Serializable]
public class CharacterInfo
{
    public string characterName;
    public int characterLevel = 1;
    public int upgradeCost = 200; // レベルアップに必要な金額
    public Sprite characterIcon;
    public WeaponInfo equippedWeapon; // 装備中の武器
}

public class CharacterManager : MonoBehaviour
{
    public CharacterInfo[] characters; // Inspectorでキャラ情報を編集
    public WeaponInfo[] weapons;       // Inspectorで武器情報を編集
    public int playerMoney = 1000;     // 所持金（Inspectorで編集可）

    public int selectedCharacterIndex = 0; // 出撃キャラ選択
    public int selectedWeaponIndex = 0;    // 装備武器選択

    // キャラのレベルアップ
    public void UpgradeCharacter(int charIndex)
    {
        var chara = characters[charIndex];
        if (playerMoney >= chara.upgradeCost)
        {
            playerMoney -= chara.upgradeCost;
            chara.characterLevel++;
        }
    }

    // 武器のレベルアップ
    public void UpgradeWeapon(int weaponIndex)
    {
        var weapon = weapons[weaponIndex];
        if (playerMoney >= weapon.upgradeCost)
        {
            playerMoney -= weapon.upgradeCost;
            weapon.weaponLevel++;
        }
    }

    // キャラの装備変更
    public void EquipWeaponToCharacter(int charIndex, int weaponIndex)
    {
        characters[charIndex].equippedWeapon = weapons[weaponIndex];
    }

    // ステージ出撃キャラ変更
    public void SelectCharacter(int charIndex)
    {
        selectedCharacterIndex = charIndex;
    }
}
