using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�ܺ� Dll �ҷ�����...
using MySql.Data;
using MySql.Data.MySqlClient;
using LitJson;

// DB�� ������ ���̺� Classȭ
public class User_info
{
    public string User_name { get; private set; }
    public string User_Password { get; private set; }

    public User_info(string name, string password)
    {
        User_name = name;
        User_Password = password;
    }
}
public class SQLManager : MonoBehaviour
{
    public User_info info;

    //SQL �����ϱ� ���� ������
    //������ ���������� �ϴ� ���̸�, ���� ���¸� Ȯ���� �� ���
    public MySqlConnection con;
    //�����͸� ���������� �о���� ���Դϴ�... reader�� �ѹ� ����ϸ� �ݵ�� �ݾ���� ���� �������� ������.
    public MySqlDataReader reader;

    public string DB_Path = string.Empty;

    public static SQLManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DB_Path = Application.dataPath + "/Database";
        string serverinfo = DBserverSet(DB_Path);

        try
        {
            if (serverinfo.Equals(string.Empty))
            {
                Debug.Log("SQL server Json Error!");
                return;
            }
            con = new MySqlConnection(serverinfo);  // ���� ���� ����
            con.Open(); // ���� ����
            Debug.Log("SQL server Open complete!");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private string DBserverSet(string path)
    {
        /*
         [{"IP":"192.168.100.31",
         "TableName":"programming",
        "ID":"root",
        "PW":"1234",
        "PORT":"3306"}]
         */
        if (!File.Exists(path))  //�� ��ο� ������ �ֳ���?
        {
            Directory.CreateDirectory(path);    // ���� ����
        }
        string jsonstring = File.ReadAllText(path + "/config.json");
        JsonData data = JsonMapper.ToObject(jsonstring);
        string serverinfo =
            $"Server = {data[0]["IP"]};" +
            $"Database = {data[0]["TableName"]};" +
            $"Uid = {data[0]["ID"]};" +
            $"Pwd = {data[0]["PW"]};" +
            $"Port = {data[0]["PORT"]};" +
            "Charset = utf8;";

        return serverinfo;
    }

    private bool connection_check(MySqlConnection c)
    {
        if (c.State != System.Data.ConnectionState.Open)
        {
            c.Open();
            if (c.State != System.Data.ConnectionState.Open)
            {
                Debug.Log("MySqlConnection is no open...");
                return false;
            }
        }
        return true;
    }

    public bool Login(string id, string passwoard)
    {
        // ���������� DB���� �����͸� ������ ���� �޼ҵ�
        // ��ȸ�Ǵ� �����Ͱ� ���ٸ� false
        // ��ȸ�Ǵ� �����Ͱ� �ִٸ� true �����µ�
        // ������ ĳ���س� info���ٰ� ��Ƽ� ĳ���س��� �̴ϴ�...
        /*
         1. connect�� Ȯ�� -> �޼ҵ�ȭ
         2. reader ���°� �а� �ִ� ��Ȳ���� Ȯ��
            - �� �������� �Ѱ���
         3. �����͸� ���о����� reader�� ���¸� Ȯ�� �� close ��!! �ؾ��մϴ�. 
         */
        try
        {
            //1. connect�� Ȯ�� -> �޼ҵ�ȭ
            if (!connection_check(con))
            {
                return false;
            }
            // ������
            //SELECT User_Name, User_Password, User_PhoneNum FROM user_info WHERE User_Name='������' AND user_password='1234';
            string sqlcommend =
                string.Format(@"SELECT Name, Password  FROM user_info WHERE Name='{0}' AND Password='{1}';", id, passwoard);

            MySqlCommand cmd = new MySqlCommand(sqlcommend, con);   // �������� ����� DB�� ������ ���� ��ü
            reader = cmd.ExecuteReader();
            // reader�� ���� �����Ͱ� 1�� �̻� ������?
            if (reader.HasRows)
            {
                // ���� �����͸� ����
                while (reader.Read())
                {
                    /*���׿�����*/
                    string name = (reader.IsDBNull(0)) ? string.Empty : reader["Name"].ToString();
                    string pwd = (reader.IsDBNull(1)) ? string.Empty : reader["Password"].ToString();

                    if (!name.Equals(string.Empty) || !pwd.Equals(string.Empty))
                    {
                        info = new User_info(name, pwd);
                        if (!reader.IsClosed) reader.Close();
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }//while�� ��
            }//if ��
            if (!reader.IsClosed) reader.Close();
            return false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            if (!reader.IsClosed) reader.Close();
            return false;
        }
    }

    // temp ȸ������
    public bool Signup(string id, string password)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }
            //INSERT INTO user_info VALUES("ȫ�浿","1234","01000000000");
            string sqlsignup =
                 string.Format(@"INSERT INTO user_info VALUES('{0}','{1}')", id, password);
            MySqlCommand cmd = new MySqlCommand(sqlsignup, con);
            // ExecuteNonQuery() : ������ ���� ���� ���� ��ȯ 
            // insert -> 1(1���� �� �߰�)
            // update -> n(n���� �� ����) 
            // delete -> n (n���� �� ����)
            int result = cmd.ExecuteNonQuery(); // ������ 1 ��ȯ

            if (result > 0)
            {
                Debug.Log("ȸ������ ����");
                return true;
            }

            else
            {
                Debug.Log("ȸ������ ����");
                return false;
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    // ȸ������ ����
    public bool Updateinfo(string curid, string newid, string newpwd)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //������
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='������';
            string sqlupdate =
                string.Format(@"UPDATE user_info SET Name = '{0}', Password='{1}' WHERE Name='{2}';", newid, newpwd,  curid);
            MySqlCommand cmd = new MySqlCommand(sqlupdate, con);

            int result = cmd.ExecuteNonQuery(); // ������ n�� ��ȯ

            if (result > 0)
            {
                info = new User_info(newid, newpwd);
                Debug.Log("ȸ������ ���� �Ϸ�");
                return true;
            }

            else
            {
                Debug.Log("������ �� �����ϴ�.");
                return false;
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    // ȸ��Ż��
    public bool Deleteinfo(string id, string password)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //������
            //DELETE FROM user_info WHERE User_Name='������';
            string deletesql =
                string.Format(@"DELETE FROM user_info WHERE Name='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(deletesql, con);

            int result = cmd.ExecuteNonQuery(); //������ n�� ��

            if (result > 0)
            {
                Debug.Log($"{id}�� �����Ǿ����ϴ�.");
                info = null;
                return true;
            }

            else
            {
                Debug.Log("���� �����߽��ϴ�.");
                return false;
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }
}

