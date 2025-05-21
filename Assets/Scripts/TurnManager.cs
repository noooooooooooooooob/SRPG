using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum TurnState
{
    Generate,
    Ready,
    Acting,
    Battle
}
public class TurnManager : MonoBehaviour
{
    public List<GameObject> playerList;
    public List<GameObject> enemyList;
    public TurnState curState = TurnState.Generate;
    public MapManager mapManager;
    public CharacterSpawner characterSpawner;
    public CameraController cameraController;

    private List<Character> allCharacters = new();
    private Character currentCharacter;
    private List<Character> turnQueue = new(); // 턴을 잡을 캐릭터들
    public bool isActing = false;

    [Header("Stage Effect UI")]
    public CanvasGroup fadeGroup;         // 검은 화면
    public TextMeshProUGUI stageText;     // "STAGE 1 START" 텍스트
    void Awake()
    {
        StartCoroutine(TurnStateMachine());
    }

    IEnumerator TurnStateMachine()
    {
        while (true)
        {
            switch (curState)
            {
                case TurnState.Generate:
                    Debug.Log("🛠 맵 생성 중...");
                    yield return StartCoroutine(PlayStageIntroEffect());
                    yield return StartCoroutine(mapManager.GenerateMapCoroutine());

                    foreach (var ch in characterSpawner.spawnedCharacters)
                    {
                        allCharacters.Add(ch);
                        ch.ResetGauge();
                    }
                    curState = TurnState.Ready;
                    break;
                case TurnState.Ready:
                    Debug.Log("Ready 상태 전환");
                    yield return StartCoroutine(HandleReadyState());
                    break;
                case TurnState.Acting:
                    yield return StartCoroutine(HandleActingState());
                    break;
                case TurnState.Battle:
                    yield return null;
                    break;
            }
            yield return null;
        }
    }
    // 스테이지 시작시 예시 연출
    IEnumerator PlayStageIntroEffect()
    {
        fadeGroup.alpha = 0;
        fadeGroup.gameObject.SetActive(true);
        stageText.gameObject.SetActive(true);

        stageText.text = $"STAGE 1 START";
        stageText.alpha = 0;

        Sequence seq = DOTween.Sequence();

        // 페이드 인 + 텍스트 등장
        seq.Append(fadeGroup.DOFade(1, 0.8f));
        seq.Join(stageText.DOFade(1, 0.8f));

        // 잠시 정지
        seq.AppendInterval(1.2f);

        // 텍스트 사라짐 + 페이드 아웃
        seq.Append(stageText.DOFade(0, 0.5f));
        seq.Join(fadeGroup.DOFade(0, 0.5f));

        seq.Play();

        yield return seq.WaitForCompletion();

        fadeGroup.gameObject.SetActive(false);
        stageText.gameObject.SetActive(false);
    }
    IEnumerator HandleReadyState()
    {
        float tickInterval = 0.01f;
        float lastTime = Time.time;

        while (curState == TurnState.Ready)
        {
            float now = Time.time;
            float elapsed = now - lastTime;

            if (elapsed >= tickInterval)
            {
                foreach (var ch in allCharacters)
                {
                    if (ch == null || ch.isDie) continue;
                    ch.IncreaseGauge(elapsed);

                    if (ch.CanAct() && !turnQueue.Contains(ch))
                    {
                        turnQueue.Add(ch);
                    }
                }
                lastTime = now;

                if (turnQueue.Count > 0)
                {
                    curState = TurnState.Acting;
                    yield break;
                }
            }

            yield return null;
        }
    }
    IEnumerator HandleActingState()
    {
        while (turnQueue.Count > 0)
        {
            Character actingChar = turnQueue[0];
            turnQueue.RemoveAt(0);

            if (actingChar == null || actingChar.isDie) continue;

            Debug.Log($" {actingChar.name} 행동 선택 대기");
            cameraController.ZoomToCharacterTile(actingChar.currentTile);

            actingChar.HasSelectedAction = false;
            actingChar.ShowActionUI(true);

            yield return new WaitUntil(() => actingChar.HasSelectedAction);
            actingChar.ShowActionUI(false);

            yield return new WaitUntil(() => !isActing);
        }

        curState = TurnState.Ready;
        yield return null;
    }
    public void removeDeadCharacter(Character dead)
    {
        allCharacters.Remove(dead);
    }
}
