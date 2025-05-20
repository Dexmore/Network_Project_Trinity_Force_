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
    public string User_Password { get; set; }
    public string User_Nickname { get; set; }

    public User_info(string name, string password, string nickname)
    {
        User_name = name;
        User_Password = password;
        User_Nickname = nickname;
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
                string.Format(@"SELECT Name, Password, Nickname  FROM user_info WHERE Name='{0}' AND Password='{1}';", id, passwoard);

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
                    string nickname = (reader.IsDBNull(1)) ? string.Empty : reader["Nickname"].ToString();

                    if (!name.Equals(string.Empty) || !pwd.Equals(string.Empty))
                    {
                        info = new User_info(name, pwd, nickname);

                        if(nickname == null)
                        {
                            return false;
                        }

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

    // ȸ������
    // 1�ܰ� : ���̵� , ��й�ȣ�� �Է��Ѵ�. ���̵� �ߺ��� �ȵ� �� 2�ܰ�� �̵��Ѵ�
    // 2�ܰ� : �г����� �Է��Ѵ�. �г��ӵ� �ߺ��� ������ �ʴ´�. �ߺ��� �ȵǸ� ȸ������ �Ϸ�
    public bool SignupStep1(string id, string password)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //1. ���̵� �ߺ� Ȯ��
            // �г����� nulll���� �ƴ���  -> ���� ����
            string sqlcheck =
                string.Format(@"SELECT Nickname FROM user_info WHERE Name = '{0}';", id);
            MySqlCommand checkcmd = new MySqlCommand(sqlcheck, con);

            //ExecuteScalar() : ����� ��ȯ�� ù ��° ���� ù ��° �� ���� ��ȯ�ϴ� �޼ҵ� -> �� 1���� ������ �� ����
            // ������ ���� ���ʿ��� �����͸� �������� ����
            // ExecuteScalar()�� ��ȯ�Ǵ� ���� object
            object resultObj = checkcmd.ExecuteScalar();

            //2.  ���̵� �ƿ� ���ٸ�? ���� Insert
            //INSERT INTO user_info VALUES("ȫ�浿","1234","01000000000");
            if(resultObj == null)  
            {
                string sqlsignup =
                     string.Format(@"INSERT INTO user_info VALUES('{0}','{1}', NULL)", id, password);
                MySqlCommand cmd = new MySqlCommand(sqlsignup, con);
            
                // ExecuteNonQuery() : ������ ���� ���� ���� ��ȯ (����� ��ȯ���� �ʴ� ������ ���)
                // insert -> 1(1���� �� �߰�)
                // update -> n(n���� �� ����) 
                // delete -> n (n���� �� ����)
                int result = cmd.ExecuteNonQuery(); // ������ 1 ��ȯ
                
                if(result > 0)  // ���̵� ��� �Ϸ�
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }

            //3. ���̵�� �ִµ� Nickname�� null -> �簡������ ����, ����� ������Ʈ ����
            // DBNull.Value�� SQL���� NULL
            if (resultObj is DBNull)  //is�� Ÿ�� �˻�
            {
                string sqlupdate =
                    string.Format(@"UPDATE user_info SET Password = '{0}' WHERE Name = '{1}';", password, id);
                MySqlCommand cmdupdate = new MySqlCommand(sqlupdate, con);
                int updateresult = cmdupdate.ExecuteNonQuery();
                return updateresult > 0;
            }

            //4. ���̵� �ְ�, �г��� �ִ� -> �ߺ����� ����
                return false;
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    public bool SignupStep2(string nickname, string id)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            // �г��� �ߺ� Ȯ��
            string sqlcheckname =
                string.Format(@"SELECT COUNT(*) FROM user_info WHERE Nickname = '{0}';", nickname);
            MySqlCommand cmdcheck = new MySqlCommand(sqlcheckname, con);
            object resultObj = cmdcheck.ExecuteScalar();
            int count = Convert.ToInt32(resultObj);

            if(count > 0)   // �г��� �ߺ�
            {
                return false;
            }


            // 2�ܰ� : �г��� �߰�
            string sqlnickname =
                string.Format(@"UPDATE user_info SET Nickname = '{0}' WHERE Name='{1}';", nickname, id);
            MySqlCommand cmd = new MySqlCommand(sqlnickname, con);

            int result = cmd.ExecuteNonQuery();

            if (result > 0) // ȸ������ ����
            {
                return true;
            }

            else //ȸ�� ���� ����
            {
                return false;
            }
        }

        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }


    // ȸ������ ����
    // 1�ܰ� : �г��Ӱ� ��й�ȣ �� �� ���� �����Ѵ�.
    // 2�ܰ� : �г��� �����ϸ� �г��Ӹ� ����, ��й�ȣ�� �����ϸ� ��й�ȣ�� ����

    // �г��� ���� �Լ�
    public bool UpdateNicknameinfo(string id, string currentname, string newname)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }
            //�г��� �ߺ� Ȯ��
            string sqlcheckname =
                string.Format(@"SELECT COUNT(*) FROM user_info WHERE Nickname = '{0}';", newname);
            MySqlCommand checkcmd = new MySqlCommand(sqlcheckname, con);
            object resultObj = checkcmd.ExecuteScalar();
            int count = Convert.ToInt32(resultObj);

            if(count > 0)   // �г��� �ߺ� ��
            {
                return false;
            }

            //������
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='������';
            string sqlupdatename =
                string.Format(@"UPDATE user_info SET Nickname = '{0}' WHERE Name ='{1}';", newname, id);
            MySqlCommand cmd = new MySqlCommand(sqlupdatename, con);

            int result = cmd.ExecuteNonQuery(); // ������ n�� ��ȯ

            if (result > 0) // �г��� ���� �Ϸ�
            {
                info.User_Nickname = newname;
                return true;
            }

            else //���� ����
            {
                return false;
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    // ��й�ȣ ����
    public bool Updatepasswordinfo(string id, string newpassword)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //������
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='������';
            string sqlupdatepwd =
                string.Format(@"UPDATE user_info SET Password = '{0}' WHERE Name ='{1}';", newpassword, id);
            MySqlCommand cmd = new MySqlCommand(sqlupdatepwd, con);

            int result = cmd.ExecuteNonQuery(); // ������ n�� ��ȯ

            if (result > 0) //��й�ȣ ���� �Ϸ�
            {
                info.User_Password = newpassword;
                return true;
            }

            else // �������
            {
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
                info = null;
                return true;
            }

            else
            {
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

