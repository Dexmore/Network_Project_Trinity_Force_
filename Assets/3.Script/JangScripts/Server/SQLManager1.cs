using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// �ܺ� dll �ҷ�����
using MySql.Data;
using MySql.Data.MySqlClient;
using LitJson;

// DB�� ���������̺� Classȭ
public class User_info1
{
    public string User_UID { get; private set; }
    public string User_Password { get; private set; }
    public string User_Birthday { get; private set; }

    public User_info1(string name, string password, string birth)
    {
        User_UID = name;
        User_Password = password;
        User_Birthday = birth;
    }
}
public class SQLManager1 : MonoBehaviour
{
    public User_info1 info;

    // SQL �����ϱ� ���� ������
    public MySqlConnection con; // ������ ���������� �ϴ� ���̸�, ���� ���¸� Ȯ���� �� ���
    public MySqlDataReader reader; // �����͸� ���������� �о���³� reader�� �ѹ� ����ϸ� �ݵ�� �ݾ���� ���� �������� ������.

    public string DB_Path = string.Empty;

    public static SQLManager1 instance = null;
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
            con = new MySqlConnection(serverinfo); // ���� ���� ����
            con.Open(); // ���� ����
            Debug.Log("SQL server Open Compelete!");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    private string DBserverSet(string path)
    {
        /*
         [
            {"IP":"192.168.100.31",
            "TableName":"programming",
            "ID":"root",
            "PW":"1234",
            "PORT":"3306"}
        ]
         */
        if (!File.Exists(path)) // �� ��ο� ������ �ֳ���?
        {
            Directory.CreateDirectory(path); // ���� ����
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
                Debug.Log("MySqlConnection is not open...");
                return false;
            }
        }
        return true;
    }

    public bool Login(string id, string password)
    {
        // ���������� DB���� �����͸� ������ ���� �޼ҵ�
        // ��ȸ�Ǵ� �����Ͱ� ���ٸ� false
        // ��ȸ�� �Ǵ� �����Ͱ� �ִٸ� true �����µ�
        // ������ ĳ���س� info���ٰ� ��Ƽ� ĳ���س��� �̴ϴ�.
        /*
         1. connection�� Ȯ�� -> �޼ҵ�ȭ
         2. reader ���°� �а� �ִ� ��Ȳ���� Ȯ��
                            - �� �������� �Ѱ���
        3. �����͸� �� �о����� reader�� ���¸� Ȯ�� �� close ��!! �ؾ��մϴ�.
         */
        try
        {
            //  1. connection�� Ȯ�� -> �޼ҵ�ȭ
            if (!connection_check(con))
            {
                return false;
            }
            // ������
            string sqlcommend = string.Format(@"SELECT User_UID, User_Password, User_Birthday FROM User_info1
                                                                                   WHERE User_UID= '{0}' AND User_Password ='{1}';", id, password);
            MySqlCommand cmd = new MySqlCommand(sqlcommend, con); // �������� ����� DB�� ������ ���� ��ü
            reader = cmd.ExecuteReader();
            // reader�� ���� �����Ͱ� 1�� �̻� ������?
            if (reader.HasRows)
            {
                // ���� �����͸� ����
                while (reader.Read())
                {
                    /*���׿�����*/
                    string name = (reader.IsDBNull(0)) ? string.Empty : reader["User_UID"].ToString();
                    string pwd = (reader.IsDBNull(1)) ? string.Empty : reader["User_Password"].ToString();
                    string bd = (reader.IsDBNull(2)) ? string.Empty : reader["User_Birthday"].ToString();

                    if (!name.Equals(string.Empty) || !pwd.Equals(string.Empty) || !bd.Equals(string.Empty))
                    {
                        info = new User_info1(name, pwd, bd);
                        if (!reader.IsClosed) reader.Close();
                        return true;
                    }
                    else
                    {
                        break;
                    }
                } // while ��
            } // if ��
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

    public bool Register(string id, string password, string birth)
    {
        try
        {
            if (!connection_check(con)) return false;

            string checkUserSql = $"SELECT COUNT(*) FROM User_info1 WHERE User_UID = '{id}';";
            MySqlCommand checkCmd = new MySqlCommand(checkUserSql, con);
            int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (userCount > 0)
            {
                Debug.Log("User ID already exists.");
                return false;
            }

            string sqlcommend = $"INSERT INTO User_info1 (User_UID, User_Password, User_Birthday) VALUES ('{id}', '{password}', '{birth}');";

            MySqlCommand cmd = new MySqlCommand(sqlcommend, con);
            int result = cmd.ExecuteNonQuery();

            if (result > 0)
            {
                Debug.Log("Registration Successful");
                return true;
            }
            else
            {
                Debug.Log("Registration Failed");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }
    public bool UpdateUser(string id, string newPassword, string newBirth)
    {
        try
        {
            if (!connection_check(con)) return false;

            string sqlcommend = $"UPDATE User_info1 SET User_Password = '{newPassword}', User_Birthday = '{newBirth}' WHERE User_UID = '{id}';";
            MySqlCommand cmd = new MySqlCommand(sqlcommend, con);
            int result = cmd.ExecuteNonQuery();

            if (result > 0)
            {
                Debug.Log("User info updated successfully.");
                return true;
            }
            else
            {
                Debug.Log("User info update failed.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public bool DeleteUser(string id)
    {
        try
        {
            if (!connection_check(con)) return false;

            string sqlcommend = $"DELETE FROM User_info1 WHERE User_UID = '{id}';";
            MySqlCommand cmd = new MySqlCommand(sqlcommend, con);
            int result = cmd.ExecuteNonQuery();

            if (result > 0)
            {
                Debug.Log("User deleted successfully.");
                return true;
            }
            else
            {
                Debug.Log("User deletion failed.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }
}
