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

    [Header("ȸ������ 1�ܰ�: ���̵�, ��й�ȣ �Է� + UI OnOff ����")]
    [SerializeField] GameObject Signup_pannel;  // 1�ܰ� �г�
    [SerializeField] InputField id_Signup;
    [SerializeField] InputField pwd_Signup;
    [SerializeField] private Text log1_signup;   //ȸ�����Խ�(���̵� �ߺ� ��) ��� �ȳ� ����

    [Header("ȸ������ 2�ܰ�: �г��� �Է� + UI OnOff ����")]
    private string cached_id;   //1�ܰ� ������ ���̵�
    [SerializeField] GameObject Nickname_pannel;    // 2�ܰ� �г�
    [SerializeField] InputField nickname_Signup;    // 2�ܰ迡�� ����� �г��� �Է� �ʵ�
    [SerializeField] private Text log2_signup;   //ȸ�� ���Խ�(�г��� �ߺ� ��) ��� �ȳ� ����

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
            User_info info = SQLManager.instance.info;

        // ���� �α��� �� nickname�� null�̶��
            if (string.IsNullOrEmpty(SQLManager.instance.info.User_Nickname))
            {
                cached_id = id_i.text;
                //�г��� �Է� �г��� �����ش�.
                Nickname_pannel.SetActive(true);

                // �г��� �Է�â �ʱ�ȭ
                nickname_Signup.text = string.Empty;
                log2_signup.text = string.Empty;
                return;
            }
            //�α��� ���� ��
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

        //�Է�â �ʱ�ȭ
        id_Signup.text = string.Empty;
        pwd_Signup.text = string.Empty;
        log1_signup.text = string.Empty;
    }

    public void CloseSignupPannel()
    {
        Signup_pannel.gameObject.SetActive(false);

    }

    // 1�ܰ� ���̵�, ��� ��ȣ �Է� -> �ߺ��� �ȵǸ� 2�ܰ�� ���� ��ư
    public void CheckIDAndOPenNicknamePannelBtn()
    {
        if (id_Signup.text.Equals(string.Empty) || pwd_Signup.text.Equals(string.Empty))
        {
            log1_signup.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
            return;
        }

        if (SQLManager.instance.SignupStep1(id_Signup.text, pwd_Signup.text))
        {
            // 2�ܰ迡 �Է��� ���̵� ���� 
            cached_id = id_Signup.text;

            // 1�ܰ� ���� + ���
            Debug.Log($"{id_Signup.text} �Է��ϼ̽��ϴ�.");
            Signup_pannel.gameObject.SetActive(false);
            Nickname_pannel.gameObject.SetActive(true);

            // �г��� �Է�â �ʱ�ȭ
            nickname_Signup.text = string.Empty;
            log2_signup.text = string.Empty;
        }

        else
        {
            log1_signup.text = "�̹� �����ϴ� ���̵��Դϴ�.";
            Debug.Log("�ٽ� Ȯ���ϼ���");
        }
    }

    // 2�ܰ� �г��� �Է� -> �ߺ��� �ȵǸ� ȸ�� ���� �Ϸ�
    public void CompleteSignup()
    {
        if (nickname_Signup.text.Equals(string.Empty))
        {
            log2_signup.text = "�г����� �Է��ϼ���";
            return;
        }

        if(SQLManager.instance.SignupStep2(nickname_Signup.text, cached_id))
        {
            // 2�ܰ� ���� + ȸ������ �Ϸ�
            Debug.Log($"{nickname_Signup.text}�� ���� �Ϸ�");
            Nickname_pannel.gameObject.SetActive(false);

        }

        else
        {
            log2_signup.text = "�̹� �����ϴ� �г����Դϴ�";
            Debug.Log("�ٽ� Ȯ���ϼ���");
        }
    }

    public void CloseNicknamePannel()
    {
        Nickname_pannel.gameObject.SetActive(false);
    }
    #endregion
}
