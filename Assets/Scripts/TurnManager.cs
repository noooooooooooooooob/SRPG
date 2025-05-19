using System.Collections.Generic;
using UnityEngine;
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
    public bool isActing = false;
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
                    ch.IncreaseGauge(elapsed); // 실제 흐른 시간만큼 증가
                    if (ch.CanAct())
                    {
                        currentCharacter = ch;
                        curState = TurnState.Acting;
                        yield break;
                    }
                }
                lastTime = now;
            }

            yield return null;
        }
    }
    IEnumerator HandleActingState()
    {
        Debug.Log($" {currentCharacter.name} 행동 선택 대기");
        cameraController.ZoomToCharacterTile(currentCharacter.currentTile);

        currentCharacter.HasSelectedAction = false; // 초기화 필요
        currentCharacter.ShowActionUI(true); // 패널 띄우기
        yield return new WaitUntil(() => currentCharacter.HasSelectedAction);
        currentCharacter.ShowActionUI(false); //  패널 숨기기

        yield return new WaitUntil(() => !isActing);
        curState = TurnState.Ready;
        yield return null;
    }
    public void removeDeadCharacter(Character dead)
    {
        allCharacters.Remove(dead);
    }
}
