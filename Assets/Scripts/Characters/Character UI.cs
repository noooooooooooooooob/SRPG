using UnityEngine;
using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] Character character; // 캐릭터 스크립트
    public GameObject actionPanel; // 캐릭터 전용 UI
    public GameObject skillPanel; // 스킬 UI
    public GameObject actionPanelReturnPanel; // 행동 취소
    private CanvasGroup actionPanelCanvasGroup; // UI 애니메이션용
    private CanvasGroup actionPanelReturnPanelCanvasGroup; // UI 애니메이션용
    private Vector3 originalScale; // UI 애니메이션용
    private Vector2 originalAnchoredPos; // UI 애니메이션용
    public UnityEngine.UI.Image turnGaugeImage;
    public UnityEngine.UI.Image HPBarImage;
    private void Start()
    {
        if (actionPanel != null)
        {
            // 원래 스케일 저장
            originalScale = actionPanel.transform.localScale;

            // CanvasGroup 확인
            actionPanelCanvasGroup = actionPanel.GetComponent<CanvasGroup>();
            if (actionPanelCanvasGroup == null)
                actionPanelCanvasGroup = actionPanel.AddComponent<CanvasGroup>();

            // 초기 상태: 안 보이게 설정
            actionPanelCanvasGroup.alpha = 0f;
            actionPanel.SetActive(false);

            // CanvasGroup 확인
            actionPanelReturnPanelCanvasGroup = actionPanelReturnPanel.GetComponent<CanvasGroup>();
            if (actionPanelReturnPanelCanvasGroup == null)
                actionPanelReturnPanelCanvasGroup = actionPanelReturnPanel.AddComponent<CanvasGroup>();

            // 초기 상태: 안 보이게 설정
            actionPanelReturnPanelCanvasGroup.alpha = 0f;
            actionPanelReturnPanel.SetActive(false);
        }
    }
    public void ShowActionUI(bool show)
    {
        originalAnchoredPos = actionPanel.GetComponent<RectTransform>().anchoredPosition;
        RectTransform rect = actionPanel.GetComponent<RectTransform>();
        Vector2 offScreenPos = originalAnchoredPos + new Vector2(-500f, 0f); // 왼쪽으로 이동

        if (show)
        {
            actionPanel.SetActive(true);
            actionPanelCanvasGroup.alpha = 0f;
            actionPanelCanvasGroup.blocksRaycasts = true;

            rect.anchoredPosition = offScreenPos;
            rect.DOAnchorPos(originalAnchoredPos, 0.3f).SetEase(Ease.OutCubic);
            actionPanelCanvasGroup.DOFade(1f, 0.3f);
        }
        else
        {
            actionPanelCanvasGroup.blocksRaycasts = false;

            rect.DOAnchorPos(offScreenPos, 0.2f).SetEase(Ease.InCubic);
            actionPanelCanvasGroup.DOFade(0f, 0.2f)
                .OnComplete(() => actionPanel.SetActive(false));
        }
    }
    public void ShowActionReturnUI(bool show)
    {
        originalAnchoredPos = actionPanelReturnPanel.GetComponent<RectTransform>().anchoredPosition;

        RectTransform rect = actionPanelReturnPanel.GetComponent<RectTransform>();
        Vector2 offScreenPos = originalAnchoredPos + new Vector2(-500f, 0f); // 왼쪽 오프스크린 위치

        if (show)
        {
            actionPanelReturnPanel.SetActive(true);
            actionPanelReturnPanelCanvasGroup.alpha = 0f;
            actionPanelReturnPanelCanvasGroup.blocksRaycasts = true;

            rect.anchoredPosition = offScreenPos; // 위치 초기화
            rect.DOAnchorPos(originalAnchoredPos, 0.3f).SetEase(Ease.OutCubic); // 슬라이드 인
            actionPanelReturnPanelCanvasGroup.DOFade(1f, 0.3f); // 페이드 인
        }
        else
        {
            actionPanelReturnPanelCanvasGroup.blocksRaycasts = false;

            rect.DOAnchorPos(offScreenPos, 0.2f).SetEase(Ease.InCubic); // 슬라이드 아웃
            actionPanelReturnPanelCanvasGroup.DOFade(0f, 0.2f) // 페이드 아웃
                .OnComplete(() => actionPanelReturnPanel.SetActive(false));
        }
    }
    // UI 메소드
    public void OnClickMove() // Move 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        ShowActionReturnUI(true);
        character.ShowMovableTiles();
    }
    public void OnClickAttack() // Attack 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        ShowActionReturnUI(true);
        character.ShowAttackableTiles();
    }
    public void OnClickSkill() // Skill 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        showSkillUI(true);
        ShowActionReturnUI(true);
    }
    public void OnClickRest() // Rest 클릭 시
    {
        ShowActionUI(false); // UI 닫기
        character.SPHeal(30);
        character.ResetGauge();
    }
    public void OnClickReturn() // 행동 취소 시
    {
        ShowActionReturnUI(false);
        showSkillUI(false);
        character.ClearPreviousTiles();
        ShowActionUI(true);
    }
    public void showSkillUI(bool show)
    {
        skillPanel.SetActive(show);
    }
    public void SetGaugeImage(float amount)
    {
        turnGaugeImage.fillAmount = amount / 100f;
    }
    public void SetHpBarImage(float amount)
    {
        HPBarImage.DOFillAmount(amount, 0.3f);
    }
}
