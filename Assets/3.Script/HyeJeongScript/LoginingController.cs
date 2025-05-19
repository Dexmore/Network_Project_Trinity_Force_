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

    [Header("ȸ������ ����")]
    [SerializeField] private GameObject Update_pannel;
    public InputField name_new;
    public InputField pwd_new;
    [SerializeField] private Text editlog;  // ȸ������ ���� ��� �ȳ� ����

    [Header("ȸ��Ż��")]
    public GameObject Delete_Pannel;

    //�α׾ƿ�
    public void LogoutBtn()
    {
        loginingPannel.SetActive(false);
        login.gameObject.SetActive(true);
        Debug.Log("�α׾ƿ� �Ǿ����ϴ�");
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

    public void UpdateBtn()
    {
        if (SQLManager.instance.info == null)
        {
            Debug.Log("�α��� ���� �ϼ���");
            return;
        }
    
        string id = SQLManager.instance.info?.User_name;
        string currentname = SQLManager.instance.info?.User_Nickname;
        string newname = name_new.text;
        string newpwd = pwd_new.text;

        //temp
        Debug.Log(currentname);

        if(name_new.text.Equals(string.Empty) || pwd_new.text.Equals(string.Empty))
        {
            editlog.text = "���̵�� ��й�ȣ�� ��� �Է��ϼ���";
            return;
        }

        if(SQLManager.instance.Updateinfo(id, currentname, newname, newpwd))
        {
            Debug.Log($"�г����� {newname}�� ����Ǿ����ϴ�.");
            Update_pannel.gameObject.SetActive(false);
            name_new.text = string.Empty;
            pwd_new.text = string.Empty;
            editlog.text = string.Empty;
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
            Debug.Log("�α��� ���� �ϼ���");
            return;
        }

        string id = SQLManager.instance.info.User_name;
        string pwd = SQLManager.instance.info.User_Password;

        if(SQLManager.instance.Deleteinfo(id, pwd))
        {
            Debug.Log("ȸ���� Ż��Ǿ����ϴ�. �׵��� �̿����ּż� �����մϴ�.");
            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);
            login.gameObject.SetActive(true);
            EmptyLoginField();
        }

        else
        {
            Debug.Log("Ż�� ����");
        }
    }
    #endregion

    //temp
    public void EmptyLoginField()
    {
        //temp
        login.id_i.text = string.Empty;
        login.pwd_i.text = string.Empty;
        login.log.text = string.Empty;
    }
}
