using System;
using UnityEngine;

/// <summary>
/// ガチャ石の保存・加算・消費を一元管理する。
/// </summary>
public static class PlayerStoneWallet
{
    private const string StoneAmountKey = "PlayerStoneAmount_v1";
    private const int DefaultStoneAmount = 0;

    public static event Action<int> AmountChanged;

    public static int Amount => Mathf.Max(0, PlayerPrefs.GetInt(StoneAmountKey, DefaultStoneAmount));

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        SetAmount(Amount + amount);
    }

    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Amount < amount)
        {
            return false;
        }

        SetAmount(Amount - amount);
        return true;
    }

    public static void SetAmount(int amount)
    {
        int value = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(StoneAmountKey, value);
        PlayerPrefs.Save();
        AmountChanged?.Invoke(value);
    }
}
