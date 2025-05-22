using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logincontroller : MonoBehaviour
{
    public InputField id_i;
    public InputField Password_i;

    [SerializeField] private Text log;

    public void LoginBtn()
    {
        if(id_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
            return;
        }

        if(SQLManager1.instance.Login(id_i.text, Password_i.text))
        {
            // �α����� ����
            User_info1 info = SQLManager1.instance.info;
            Debug.Log(info.User_UID + " | " + info.User_Birthday);
            gameObject.SetActive(false);
        }
        else
        {
            log.text = "���̵�� ��й�ȣ�� Ȯ���� �ּ���~";
        }
    }
}
