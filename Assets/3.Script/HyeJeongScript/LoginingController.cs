using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginingController : MonoBehaviour
{
    [Header("로그아웃 하기 위해 LoginController를 가져옴")]
    [SerializeField] private LoginController login;

    [Header("로그인 후 회원정보 패널")]
    public GameObject loginingPannel;

    [Header("회원정보 수정")]
    [SerializeField] private GameObject Update_pannel;
    public InputField name_new;
    public InputField pwd_new;
    [SerializeField] private Text editlog;  // 회원정보 수정 경고 안내 문구

    [Header("회원탈퇴")]
    public GameObject Delete_Pannel;

    //로그아웃
    public void LogoutBtn()
    {
        loginingPannel.SetActive(false);
        login.gameObject.SetActive(true);
        Debug.Log("로그아웃 되었습니다");
        //temp
        EmptyLoginField();
    }

    #region 회원정보 수정
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
            Debug.Log("로그인 먼저 하세요");
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
            editlog.text = "아이디와 비밀번호를 모두 입력하세요";
            return;
        }

        if(SQLManager.instance.Updateinfo(id, currentname, newname, newpwd))
        {
            Debug.Log($"닉네임이 {newname}로 변경되었습니다.");
            Update_pannel.gameObject.SetActive(false);
            name_new.text = string.Empty;
            pwd_new.text = string.Empty;
            editlog.text = string.Empty;
        }

    }
    #endregion

    #region 회원탈퇴
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
            Debug.Log("로그인 먼저 하세요");
            return;
        }

        string id = SQLManager.instance.info.User_name;
        string pwd = SQLManager.instance.info.User_Password;

        if(SQLManager.instance.Deleteinfo(id, pwd))
        {
            Debug.Log("회원이 탈퇴되었습니다. 그동안 이용해주셔서 감사합니다.");
            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);
            login.gameObject.SetActive(true);
            EmptyLoginField();
        }

        else
        {
            Debug.Log("탈퇴 실패");
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
