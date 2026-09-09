using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CharacterPlazaController : MonoBehaviour
{
    [Header("プレイヤー（NPCとは別に指定）")]
    [SerializeField] Transform player;
    [SerializeField] SpriteRenderer playerRenderer;
    [SerializeField] CharacterData playerCharacter;
    [SerializeField] bool excludePlayerCharacterFromNpcs = true;
    [Header("キャラクター画像の共通サイズ")]
    [Tooltip("全キャラクター画像の長い辺を、このワールドサイズに揃えます")]
    [Min(.1f)] [SerializeField] float characterVisualWorldSize = 2f;
    [Header("有限の広場")]
    [Tooltip("広場全体を囲むBoxCollider2D（Is TriggerをON）")]
    [SerializeField] BoxCollider2D plazaBounds;
    [Header("ランダムNPC")]
    [SerializeField] CharacterPlazaNpc npcPrefab;
    [SerializeField] Transform npcRoot;
    [Min(0)] [SerializeField] int npcCount = 5;
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] List<CharacterData> additionalNpcCandidates = new List<CharacterData>();
    [SerializeField] bool allowDuplicateCharacters;
    [Min(0)] [SerializeField] float boundsPadding = .5f;
    [Header("近距離会話")]
    [Min(.1f)] [SerializeField] float interactionDistance = 2f;
    [SerializeField] Button talkButton;
    [SerializeField] GameObject conversationPanel;
    [SerializeField] Text speakerNameText;
    [SerializeField] Text conversationText;
    [SerializeField] KeyCode keyboardTalkKey = KeyCode.E;
    [Tooltip("TouchMove2/WASDMoveDebug以外の移動スクリプトがある場合に登録します")]
    [SerializeField] Behaviour[] additionalPlayerMovementScripts;
    [Header("シーン移動")]
    [SerializeField] string homeSceneName = "HomeScene";

    readonly List<CharacterPlazaNpc> npcs = new List<CharacterPlazaNpc>();
    readonly List<Behaviour> pausedPlayerMovement = new List<Behaviour>();
    readonly List<CharacterPlazaNpc> pausedNpcs = new List<CharacterPlazaNpc>();
    CharacterPlazaNpc nearestNpc;
    bool conversationOpen;

    void Start()
    {
        if (playerRenderer && playerCharacter)
        {
            playerRenderer.sprite = playerCharacter.characterSprite;
            playerRenderer.color = playerCharacter.themeColor;
            NormalizeRenderer(playerRenderer, characterVisualWorldSize);
        }
        if (player && plazaBounds)
        {
            var limiter = player.GetComponent<CharacterPlazaPlayerBoundary>();
            if (!limiter) limiter = player.gameObject.AddComponent<CharacterPlazaPlayerBoundary>();
            limiter.Configure(plazaBounds, boundsPadding);
        }
        SpawnNpcs();
        if (talkButton)
        {
            talkButton.onClick.RemoveListener(TalkToNearestNpc);
            talkButton.onClick.AddListener(TalkToNearestNpc);
            talkButton.interactable = false;
        }
        if (conversationPanel) conversationPanel.SetActive(false);
    }

    void Update()
    {
        if (conversationOpen) return;
        FindNearest();
        if (nearestNpc && Input.GetKeyDown(keyboardTalkKey)) TalkToNearestNpc();
    }

    void SpawnNpcs()
    {
        if (!npcPrefab || !plazaBounds)
        {
            Debug.LogWarning("CharacterPlaza: Npc PrefabとPlaza Boundsを設定してください。", this);
            return;
        }
        var candidates = CollectCandidates();
        if (candidates.Count == 0) return;
        Shuffle(candidates);
        int count = allowDuplicateCharacters ? npcCount : Mathf.Min(npcCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            CharacterData data = allowDuplicateCharacters ? candidates[Random.Range(0, candidates.Count)] : candidates[i];
            var npc = Instantiate(npcPrefab, SpawnPosition(i), Quaternion.identity, npcRoot ? npcRoot : transform);
            npc.name = "NPC_" + data.characterName;
            npc.Configure(data, plazaBounds, boundsPadding, characterVisualWorldSize);
            npcs.Add(npc);
        }
    }

    List<CharacterData> CollectCandidates()
    {
        var result = new List<CharacterData>();
        var database = CharacterDatabase.GetOrCreate();
        if (database)
            for (int i = 0; i < database.GetCharacterCount(); i++) AddCandidate(result, database.GetCharacter(i));
        foreach (var data in additionalNpcCandidates) AddCandidate(result, data);
        return result;
    }

    void AddCandidate(List<CharacterData> list, CharacterData data)
    {
        if (!data || (excludePlayerCharacterFromNpcs && data == playerCharacter) || list.Contains(data)) return;
        list.Add(data);
    }

    Vector3 SpawnPosition(int index)
    {
        if (spawnPoints.Count > 0 && spawnPoints[index % spawnPoints.Count])
            return spawnPoints[index % spawnPoints.Count].position;
        Bounds b = plazaBounds.bounds;
        float p = boundsPadding;
        return new Vector3(Random.Range(b.min.x + p, b.max.x - p), Random.Range(b.min.y + p, b.max.y - p), 0);
    }

    void FindNearest()
    {
        nearestNpc = null;
        if (!player) return;
        float nearest = interactionDistance * interactionDistance;
        foreach (var npc in npcs)
        {
            if (!npc) continue;
            float distance = (npc.transform.position - player.position).sqrMagnitude;
            if (distance <= nearest) { nearest = distance; nearestNpc = npc; }
        }
        if (talkButton) talkButton.interactable = nearestNpc;
    }

    public void TalkToNearestNpc()
    {
        if (!nearestNpc || !nearestNpc.Data) return;
        CharacterData data = nearestNpc.Data;
        string phrase = "こんにちは！";
        if (data.homeTouchPhrases != null && data.homeTouchPhrases.Length > 0)
            phrase = data.homeTouchPhrases[Random.Range(0, data.homeTouchPhrases.Length)];
        if (speakerNameText) speakerNameText.text = data.characterName;
        if (conversationText) conversationText.text = phrase;
        if (conversationPanel) conversationPanel.SetActive(true);
        nearestNpc.PauseForConversation();
        SetConversationMovementPaused(true);
    }

    public void CloseConversation()
    {
        if (conversationPanel) conversationPanel.SetActive(false);
        SetConversationMovementPaused(false);
    }
    public void ReturnHome() { SceneManager.LoadScene(homeSceneName); }

    void SetConversationMovementPaused(bool paused)
    {
        conversationOpen = paused;
        if (paused)
        {
            pausedPlayerMovement.Clear();
            PauseMovement(player ? player.GetComponent<TouchMove2>() : null);
            PauseMovement(player ? player.GetComponent<WASDMoveDebug>() : null);
            if (additionalPlayerMovementScripts != null)
                foreach (Behaviour movement in additionalPlayerMovementScripts) PauseMovement(movement);

            if (player)
            {
                TouchMove2 touchMove = player.GetComponent<TouchMove2>();
                if (touchMove) touchMove.ClearPendingMovement();
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body) body.linearVelocity = Vector2.zero;
            }

            pausedNpcs.Clear();
            foreach (CharacterPlazaNpc npc in npcs)
            {
                if (npc && npc.enabled)
                {
                    npc.enabled = false;
                    pausedNpcs.Add(npc);
                }
            }
            if (talkButton) talkButton.interactable = false;
            return;
        }

        foreach (Behaviour movement in pausedPlayerMovement)
            if (movement) movement.enabled = true;
        pausedPlayerMovement.Clear();
        foreach (CharacterPlazaNpc npc in pausedNpcs)
            if (npc) npc.enabled = true;
        pausedNpcs.Clear();
        FindNearest();
    }

    void PauseMovement(Behaviour movement)
    {
        if (!movement || !movement.enabled || pausedPlayerMovement.Contains(movement)) return;
        movement.enabled = false;
        pausedPlayerMovement.Add(movement);
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        { int j = Random.Range(0, i + 1); T value = list[i]; list[i] = list[j]; list[j] = value; }
    }

    static void NormalizeRenderer(SpriteRenderer renderer, float targetWorldSize)
    {
        if (!renderer || !renderer.sprite) return;
        Vector2 spriteSize = renderer.sprite.bounds.size;
        float longestSide = Mathf.Max(spriteSize.x, spriteSize.y);
        if (longestSide <= .0001f) return;
        float multiplier = Mathf.Max(.1f, targetWorldSize) / longestSide;
        renderer.transform.localScale = Vector3.one * multiplier;
    }

    void OnValidate()
    { npcCount = Mathf.Max(0, npcCount); interactionDistance = Mathf.Max(.1f, interactionDistance); boundsPadding = Mathf.Max(0, boundsPadding); characterVisualWorldSize = Mathf.Max(.1f, characterVisualWorldSize); }
}
