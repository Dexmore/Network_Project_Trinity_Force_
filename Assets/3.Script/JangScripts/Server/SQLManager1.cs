using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 외부 dll 불러오기
using MySql.Data;
using MySql.Data.MySqlClient;
using LitJson;

// DB의 데이터테이블 Class화
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

    // SQL 연결하기 위한 변수들
    public MySqlConnection con; // 연결을 직접적으로 하는 놈이며, 연결 상태를 확인할 때 사용
    public MySqlDataReader reader; // 데이터를 직접적으로 읽어오는놈 reader는 한번 사용하면 반드시 닫아줘야 다음 쿼리문이 동작함.

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
            con = new MySqlConnection(serverinfo); // 서버 정보 생성
            con.Open(); // 서버 접근
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
        if (!File.Exists(path)) // 그 경로에 파일이 있나요?
        {
            Directory.CreateDirectory(path); // 폴더 생성
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
        // 직접적으로 DB에서 데이터를 가지고 오는 메소드
        // 조회되는 데이터가 없다며 false
        // 조회가 되는 데이터가 있다면 true 던지는데
        // 위에서 캐싱해논 info에다가 담아서 캐싱해놓을 겁니다.
        /*
         1. connection을 확인 -> 메소드화
         2. reader 상태가 읽고 있는 상황인지 확인
                            - 한 쿼리문당 한개씩
        3. 데이터를 다 읽었으면 reader의 상태를 확인 후 close 꼭!! 해야합니다.
         */
        try
        {
            //  1. connection을 확인 -> 메소드화
            if (!connection_check(con))
            {
                return false;
            }
            // 쿼리문
            string sqlcommend = string.Format(@"SELECT User_UID, User_Password, User_Birthday FROM User_info1
                                                                                   WHERE User_UID= '{0}' AND User_Password ='{1}';", id, password);
            MySqlCommand cmd = new MySqlCommand(sqlcommend, con); // 쿼리문을 연결된 DB에 날리기 위한 객체
            reader = cmd.ExecuteReader();
            // reader가 읽은 데이터가 1개 이상 존재해?
            if (reader.HasRows)
            {
                // 읽은 데이터를 나열
                while (reader.Read())
                {
                    /*삼항연산자*/
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
                } // while 끝
            } // if 끝
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
