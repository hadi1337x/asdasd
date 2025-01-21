using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager mainManager;

    public GameObject StartDialog;

    public Button startBTN;
    public Button optionBTN;
    public Button exitBTN;
    public Button cancelBTN;
    public Button loginBTN;
    public Button regBTN;

    public Toggle signUpToggle;

    public TMP_InputField username;
    public TMP_InputField password;
    public TMP_Text logTXT;

    public void StartButtonPressed()
    {
        StartDialog.SetActive(true);
    }
    public void CancelButtonPressed()
    {
        StartDialog.SetActive(false);
    }
    public void SignUpToggled()
    {
        if (signUpToggle.isOn)
        {
            regBTN.gameObject.SetActive(true);
            loginBTN.gameObject.SetActive(false);
        }
        else
        {
            regBTN.gameObject.SetActive(false);
            loginBTN.gameObject.SetActive(true);
        }
    }
    public void SignUpBTNClicked()
    {
        if(username.text != string.Empty && password.text != string.Empty)
        {
            Connection.Packet.SendRegistrationPacket(username.text, password.text);
        }
    }

    public void SignInBTNClicked()
    {
        if (username.text != string.Empty && password.text != string.Empty)
        {
            Connection.Packet.SendLoginPacket(username.text, password.text);
        }
    }
    public void SetText(string txtLogs)
    {
        logTXT.text = txtLogs;
        logTXT.gameObject.SetActive(true);
    }
}
