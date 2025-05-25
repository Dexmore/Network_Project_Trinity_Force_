using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginingController : MonoBehaviour
{
    //���̵� �ƿ�
    [SerializeField] private UIFade fade;

    [Header("�α׾ƿ� �ϱ� ���� LoginController�� ������")]
    [SerializeField] private LoginController login;

    [Header("�α��� �� ȸ������ �г�")]
    public GameObject userPannel;   // ���� �г��� �г�â
    public Text userNickname;   // ���� �г���
    public GameObject loginingPannel;   // �α׾ƿ�, ȸ����������, ȸ��Ż�� �г�â

    [Header("ȸ������ ����â")]
    [SerializeField] private GameObject Update_pannel;  // ȸ������ ���� ���� �г� -> �г���, ��й�ȣ �� 1�� ����

    [Header("ȸ������ ����â - �г���")]
    [SerializeField] private GameObject Updatename_pannel;  // �г��� ���� ���� �г�
    public InputField name_new;
    [SerializeField] private Text updatenamelog;   // �г��� ���� ��� ����

    [Header("�г��� ���� �� �˾�â")]
    [SerializeField] private GameObject NoticeNickname_pannel;  // �г��� ���� �г�â
    [SerializeField] private Text NoticeNicknamelog;    // �г��� ���� �˸�

    [Header("ȸ������ ����â - ��й�ȣ")]
    [SerializeField] private GameObject Updatepwd_pannel;  // ��й�ȣ ���� ���� �г�
    public InputField pwd_new;
    [SerializeField] private Text updatepwdlog;   // ��й�ȣ ���� ��� ����

    [Header("��й�ȣ ������ �˾�â")]
    [SerializeField] private GameObject NoticePWD_pannel;  // ��й�ȣ ���� �г�â
    [SerializeField] private Text NoticePWDlog;    // ��й�ȣ ���� �˸�

    [Header("ȸ��Ż��")]
    public GameObject Delete_Pannel;    // ȸ��Ż�� �г�â

    [Header("ȸ��Ż�� �˸�â")]
    [SerializeField] private GameObject NoticeDelete_pannel;  // ȸ��Ż�� �˸� �г�â
    [SerializeField] private Text Deletelog;   // ȸ��Ż�� �˸�����

    //�α׾ƿ�
    public void LogoutBtn()
    {
        loginingPannel.SetActive(false);
        login.LoginPannel.SetActive(true);

        //temp
        EmptyLoginField();
    }

    #region ȸ������ ����
    public void OpenUpdatePannel()
    {
        Update_pannel.SetActive(true);
    }

    public void CloseUpdatePannel()
    {
        Update_pannel.SetActive(false);
    }

    public void OpennicknamePannel()
    {
        Update_pannel.SetActive(false);
        Updatename_pannel.SetActive(true);

        //�Է�â �ʱ�ȭ
        name_new.text = string.Empty;
        updatenamelog.text = string.Empty;
    }

    public void OpenpasswordPannel()
    {
        Update_pannel.SetActive(false);
        Updatepwd_pannel.SetActive(true);

        //�Է�â �ʱ�ȭ
        pwd_new.text = string.Empty;
        updatepwdlog.text = string.Empty;
    }
    public void ClosenicknamePannel()
    {
        Updatename_pannel.SetActive(false);
    }

    public void ClosepasswordPannel()
    {
        Updatepwd_pannel.SetActive(false);
    }

    public void CloseNoticeNicknamePannel()
    {
        NoticeNickname_pannel.SetActive(false);
    }

    public void CloseNoticePWDPannel()
    {
        NoticePWD_pannel.SetActive(false);
    }

    public void CloseNoticeDeletePannel()
    {
        NoticeDelete_pannel.SetActive(false);
    }

    public void UpdateNicknameBtn()
    {
        if (SQLManager.instance.info == null)   
        {
            return;
        }

        string id = SQLManager.instance.info?.User_name;
        string currentname = SQLManager.instance.info?.User_Nickname;
        string newname = name_new.text.Trim();
        //Trim() : ��������

        if(name_new.text.Equals(string.Empty))
        {
            updatenamelog.text = "������ �г����� �Է��ϼ���";
            return;
        }

        if (SQLManager.instance.UpdateNicknameinfo(id, currentname, newname))
        {
            // �г��� ���� �˸�â ����
            Updatename_pannel.SetActive(false);
            NoticeNicknamelog.text = $"�г����� <color=yellow>{newname}</color>(��)�� ����Ǿ����ϴ�.";
            userNickname.text = newname;

            // ���̵��� ����
            fade.FadeIn(NoticeNickname_pannel);
            //NoticeNickname_pannel.SetActive(true);
            
            //�ð� ������ ���̵� �ƿ�
            StartCoroutine(fade.AutoFade(NoticeNickname_pannel, 1.5f));

            // 1.5�� �� �����
            //Invoke("CloseNoticeNicknamePannel", 1.5f);
        }

        else
        {
            updatenamelog.text = "�̹� �����ϴ� �г����Դϴ�.";
        }
    }

    public void UpdatePasswordBtn()
    {
        if (SQLManager.instance.info == null)
        {
            return;
        }

        string id = SQLManager.instance.info?.User_name;
        string newpwd = pwd_new.text.Trim();

        if(pwd_new.text.Equals(string.Empty))
        {
            updatepwdlog.text = "������ ��й�ȣ�� �Է��ϼ���";
            return;
        }

        if(SQLManager.instance.Updatepasswordinfo(id, newpwd))
        {
            // ��й�ȣ ���� �˸�â ����
            NoticePWDlog.text = "��й�ȣ�� ����Ǿ����ϴ�.";
            Updatepwd_pannel.SetActive(false);

            // ���̵��� ����
            fade.FadeIn(NoticePWD_pannel);
            //NoticePWD_pannel.SetActive(true);
            
            //�ð� ������ ���̵� �ƿ�
            StartCoroutine(fade.AutoFade(NoticePWD_pannel, 1.5f));

            // 1.5�� �� �����
            //Invoke("CloseNoticePWDPannel", 1.5f);
        }

        else
        {
            updatepwdlog.text = "�ٽ� �Է��ϼ���";
        }
    }
    #endregion

    #region ȸ��Ż��
    public void OpenDeletePannel()
    {
        Delete_Pannel.SetActive(true);
    }

    public void CloseDeletePannel()
    {
        Delete_Pannel.SetActive(false);
    }

    public void DeleteBtn()
    {
        if(SQLManager.instance.info == null)
        {
            return;
        }

        string id = SQLManager.instance.info.User_name;
        string pwd = SQLManager.instance.info.User_Password;
        string nickname = SQLManager.instance.info.User_Nickname;

        if(SQLManager.instance.Deleteinfo(id, pwd, nickname))
        {
            // ȸ�� Ż�� �˸�â ����
            Deletelog.text = "ȸ��Ż�� �Ϸ�Ǿ����ϴ�. �׵��� �̿����ּż� �����մϴ�.";

            // ���̵��� ����
            fade.FadeIn(NoticeDelete_pannel);
            //NoticeDelete_pannel.SetActive(true);

            //�ð� ������ ���̵� �ƿ�
            StartCoroutine(fade.AutoFade(NoticeDelete_pannel, 1.5f));

            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);

            login.LoginPannel.SetActive(true);

            EmptyLoginField();

            //1.5�� �� �����
            //Invoke("CloseNoticeDeletePannel", 1.5f);
        }
    }
    #endregion

    // �α��� �Է�â �ʱ�ȭ
    public void EmptyLoginField()
    {
        login.id_i.text = string.Empty;
        login.pwd_i.text = string.Empty;
        login.log.text = string.Empty;
    }  
}
