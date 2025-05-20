using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginingController : MonoBehaviour
{
    [Header("�α׾ƿ� �ϱ� ���� LoginController�� ������")]
    [SerializeField] private LoginController login;

    [Header("�α��� �� ȸ������ �г�")]
    public GameObject loginingPannel;

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
        login.gameObject.SetActive(true);

        //temp
        EmptyLoginField();
    }

    #region ȸ������ ����
    public void OpenUpdatePannel()
    {
        Update_pannel.gameObject.SetActive(true);
    }

    public void CloseUpdatePannel()
    {
        Update_pannel.gameObject.SetActive(false);
    }

    public void OpennicknamePannel()
    {
        Update_pannel.gameObject.SetActive(false);
        Updatename_pannel.gameObject.SetActive(true);

        //�Է�â �ʱ�ȭ
        name_new.text = string.Empty;
        updatenamelog.text = string.Empty;
    }

    public void OpenpasswordPannel()
    {
        Update_pannel.gameObject.SetActive(false);
        Updatepwd_pannel.gameObject.SetActive(true);

        //�Է�â �ʱ�ȭ
        pwd_new.text = string.Empty;
        updatepwdlog.text = string.Empty;
    }
    public void ClosenicknamePannel()
    {
        Updatename_pannel.gameObject.SetActive(false);
    }

    public void ClosepasswordPannel()
    {
        Updatepwd_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticeNicknamePannel()
    {
        NoticeNickname_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticePWDPannel()
    {
        NoticePWD_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticeDeletePannel()
    {
        NoticeDelete_pannel.gameObject.SetActive(false);
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
        }

        if (SQLManager.instance.UpdateNicknameinfo(id, currentname, newname))
        {
            Updatename_pannel.gameObject.SetActive(false);
            NoticeNickname_pannel.gameObject.SetActive(true);
            NoticeNicknamelog.text = $"�г����� {newname}(��)�� ����Ǿ����ϴ�.";

            // 3�� �� �����
            Invoke("CloseNoticeNicknamePannel", 3f);
        }

        else
        {
            updatenamelog.text = "�ߺ��� �г����Դϴ�.";
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
        }

        if(SQLManager.instance.Updatepasswordinfo(id, newpwd))
        {
            NoticePWD_pannel.gameObject.SetActive(true);
            NoticePWDlog.text = "��й�ȣ�� ����Ǿ����ϴ�.";
            Updatepwd_pannel.gameObject.SetActive(false);

            // 3�� �� �����
            Invoke("CloseNoticePWDPannel", 3f);
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
        Delete_Pannel.gameObject.SetActive(true);
    }

    public void CloseDeletePannel()
    {
        Delete_Pannel.gameObject.SetActive(false);
    }

    public void DeleteBtn()
    {
        if(SQLManager.instance.info == null)
        {
            return;
        }

        string id = SQLManager.instance.info.User_name;
        string pwd = SQLManager.instance.info.User_Password;

        if(SQLManager.instance.Deleteinfo(id, pwd))
        {
            NoticeDelete_pannel.gameObject.SetActive(true);
            Deletelog.text = "ȸ���� Ż��Ǿ����ϴ�. �׵��� �̿����ּż� �����մϴ�.";

            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);

            login.gameObject.SetActive(true);
            EmptyLoginField();

            //3�� �� �����
            Invoke("CloseNoticeDeletePannel", 3f);
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
