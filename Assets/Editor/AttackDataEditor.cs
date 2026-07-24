using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackData))]
public class AttackDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSection("基本情報", "attackName", "attackType");

        AttackData.AttackType type =
            (AttackData.AttackType)serializedObject.FindProperty("attackType").enumValueIndex;

        if (type == AttackData.AttackType.Charge)
        {
            DrawSection("クールタイム", "cooldownTime");
        }
        else
        {
            DrawSection("ダメージ・クールタイム", "damage", "cooldownTime");
        }

        DrawSection("攻撃判定", "hitBoxPrefab", "duration");
        DrawSection(
            "生成位置",
            "spawnDistance",
            "spawnOffset",
            "followPlayerDirection");
        DrawSection(
            "見た目",
            "scale",
            "scaleAxes",
            "rotationZ",
            "flipOnDirection");

        if (type == AttackData.AttackType.MultiHit)
        {
            DrawSection(
                "多段攻撃",
                "hitCount",
                "hitInterval",
                "updateDirectionEachHit");
        }
        else if (type == AttackData.AttackType.Charge)
        {
            DrawSection(
                "チャージ攻撃",
                "maxChargeTime",
                "minDamage",
                "maxDamage",
                "minScale",
                "maxScale");
        }

        serializedObject.ApplyModifiedProperties();
        DrawValidationHelp();
    }

    private void DrawSection(string title, params string[] propertyNames)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach (string propertyName in propertyNames)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
            }
        }
    }

    private void DrawValidationHelp()
    {
        AttackData data = (AttackData)target;
        if (data.hitBoxPrefab == null)
        {
            EditorGUILayout.HelpBox(
                "Hit Box Prefabを設定してください。",
                MessageType.Warning);
            return;
        }

        if (data.hitBoxPrefab.GetComponent<HitBox>() == null)
        {
            EditorGUILayout.HelpBox(
                "指定したPrefabにHitBoxコンポーネントがありません。",
                MessageType.Error);
        }
    }
}
