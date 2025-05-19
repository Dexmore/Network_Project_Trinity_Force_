using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [Header("�α��� �� ȸ������ ����, Ż�� â")]
    [SerializeField] private LoginingController logining;

    [Header("�α��� : ���̵� �Է��ؼ� ���� ����")]
    public InputField id_i;
    public InputField pwd_i;
    public Text log;  // �α��� ��� �ȳ� ����

    [Header("ȸ������ : UI OnOff ����")]
    [SerializeField] GameObject Signup_pannel;
    [SerializeField] InputField id_Signup;
    [SerializeField] InputField pwd_Signup;
    [SerializeField] private Text log_signup;   //ȸ�� ���Խ� ��� �ȳ� ����

    #region �α���
    public void LoginBtn()
    {
        if(id_i.text.Equals(string.Empty) || pwd_i.text.Equals(string.Empty))
        {
            log.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
            return;
        }

        if (SQLManager.instance.Login(id_i.text, pwd_i.text))
        {
            //�α��� ����
            User_info info = SQLManager.instance.info;
            Debug.Log($"{info.User_name}�� �ȳ��ϼ���");
            gameObject.SetActive(false);
            logining.loginingPannel.SetActive(true);

        }

        else
        {
            log.text = "���̵�� ��й�ȣ�� Ȯ�����ּ���";
        }
    }
    #endregion

    #region ȸ������
    // ȸ������â ��ư
    public void OpenSignupPannel()
    {
        Signup_pannel.gameObject.SetActive(true);
    }

    public void CloseSignupPannel()
    {
        Signup_pannel.gameObject.SetActive(false);
    }

    public void SignUpBtn()
    {
        if (id_Signup.text.Equals(string.Empty) || pwd_Signup.text.Equals(string.Empty))
        {
            log_signup.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
            return;
        }

        if (SQLManager.instance.Signup(id_Signup.text, pwd_Signup.text))
        {
            // ȸ������ ����
            Debug.Log($"{id_Signup.text} ���� ����");
            Signup_pannel.gameObject.SetActive(false);

            //temp
            id_Signup.text = string.Empty;
            pwd_Signup.text = string.Empty;
            log_signup.text = string.Empty;
        }

        else
        {
            Debug.Log("�ٽ� Ȯ���ϼ���");
        }
    }
    #endregion
}
