using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Top Left (Player Info)")]
    [SerializeField] private TextMeshProUGUI topLeft_1; // Lives
    [SerializeField] private TextMeshProUGUI topLeft_2; // Hp
    [SerializeField] private TextMeshProUGUI topLeft_3; // Weapon

    [Header("Top Center (Wave Countdown)")]
    [SerializeField] private TextMeshProUGUI topCenter; //Next Wave timer

    [Header("Top Right (Wave/Enemy Info)")]
    [SerializeField] private TextMeshProUGUI topRight_1;    //Enemies Alive
    [SerializeField] private TextMeshProUGUI topRight_2;    //Enemies left in wave

    static private TextMeshProUGUI _UI_TEXT_TOPLEFT_1;  // Lives
    static private TextMeshProUGUI _UI_TEXT_TOPLEFT_2;  // HP
    static private TextMeshProUGUI _UI_TEXT_TOPLEFT_3;  // Weapon
    static private TextMeshProUGUI _UI_TEXT_TOPCENTER;  // Next Wave timer
    static private TextMeshProUGUI _UI_TEXT_TOPRIGHT_1; // Enemies alive
    static private TextMeshProUGUI _UI_TEXT_TOPRIGHT_2; // Enemies left in wave

    void Awake()
    {
        _UI_TEXT_TOPLEFT_1 = topLeft_1;
        _UI_TEXT_TOPLEFT_2 = topLeft_2;
        _UI_TEXT_TOPLEFT_3 = topLeft_3;

        _UI_TEXT_TOPCENTER = topCenter;

        _UI_TEXT_TOPRIGHT_1 = topRight_1;
        _UI_TEXT_TOPRIGHT_2 = topRight_2;

        if (_UI_TEXT_TOPLEFT_1) _UI_TEXT_TOPLEFT_1.text = "Lives: --";
        if (_UI_TEXT_TOPLEFT_2) _UI_TEXT_TOPLEFT_2.text = "HP: --/--";
        if (_UI_TEXT_TOPLEFT_3) _UI_TEXT_TOPLEFT_3.text = "Weapon: --";

        if (_UI_TEXT_TOPCENTER) _UI_TEXT_TOPCENTER.text = "";   // Hidden until countdown starts

        if (_UI_TEXT_TOPRIGHT_1) _UI_TEXT_TOPRIGHT_1.text = "Enemies Alive: --";
        if (_UI_TEXT_TOPRIGHT_2) _UI_TEXT_TOPRIGHT_2.text = "Left in Wave: --";
    }

    public static void SetPlayerInfo(int lives, int hpNow, int hpMax, string weaponName)
    {
        if (_UI_TEXT_TOPLEFT_1) _UI_TEXT_TOPLEFT_1.text = $"Lives: {lives}";
        if (_UI_TEXT_TOPLEFT_2) _UI_TEXT_TOPLEFT_2.text = $"HP: {hpNow}/{hpMax}";
        if (_UI_TEXT_TOPLEFT_3) _UI_TEXT_TOPLEFT_3.text = $"Weapon: {weaponName}";
    }

    public static void SetHealthOnly(int lives, int hpNow, int hpMax)
    {
        if (_UI_TEXT_TOPLEFT_1) _UI_TEXT_TOPLEFT_1.text = $"Lives: {lives}";
        if (_UI_TEXT_TOPLEFT_2) _UI_TEXT_TOPLEFT_2.text = $"HP: {hpNow}/{hpMax}";
    }

    public static void SetWaveCountdown(float secondsLeft)
    {
        if (_UI_TEXT_TOPCENTER) _UI_TEXT_TOPCENTER.text = $"Next Wave: {secondsLeft: 0.0} sec";
    }

    public static void ClearWaveCountdown()
    {
        if (_UI_TEXT_TOPCENTER) _UI_TEXT_TOPCENTER.text = "";
    }

    public static void SetEnemyInfo(int aliveNow, int waveLeftTotal)
    {
        if (_UI_TEXT_TOPRIGHT_1) _UI_TEXT_TOPRIGHT_1.text = $"Enemies Alive: {aliveNow}";
        if (_UI_TEXT_TOPRIGHT_2) _UI_TEXT_TOPRIGHT_2.text = $"Left in Wave: {waveLeftTotal}";
    }
}
