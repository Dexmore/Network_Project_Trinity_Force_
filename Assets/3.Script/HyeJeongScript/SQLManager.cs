using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//외부 Dll 불러오기...
using MySql.Data;
using MySql.Data.MySqlClient;
using LitJson;

// DB의 데이터 테이블 Class화
public class User_info
{
    public string User_name { get; private set; }
    public string User_Password { get; private set; }
    public string User_Nickname { get; private set; }

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

    //SQL 연결하기 위한 변수들
    //연결을 직접적으로 하는 놈이며, 연결 상태를 확인할 때 사용
    public MySqlConnection con;
    //데이터를 직접적으로 읽어오는 놈입니다... reader는 한번 사용하면 반드시 닫아줘야 다음 쿼리문이 동작함.
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
            con = new MySqlConnection(serverinfo);  // 서버 정보 생성
            con.Open(); // 서버 접근
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
        if (!File.Exists(path))  //그 경로에 파일이 있나요?
        {
            Directory.CreateDirectory(path);    // 폴더 생성
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
        // 직접적으로 DB에서 데이터를 가지고 오는 메소드
        // 조회되는 데이터가 없다면 false
        // 조회되는 데이터가 있다면 true 던지는데
        // 위에서 캐싱해논 info에다가 담아서 캐싱해놓을 겁니다...
        /*
         1. connect을 확인 -> 메소드화
         2. reader 상태가 읽고 있는 상황인지 확인
            - 한 쿼리문당 한개식
         3. 데이터를 다읽었으면 reader의 상태를 확인 후 close 꼭!! 해야합니다. 
         */
        try
        {
            //1. connect을 확인 -> 메소드화
            if (!connection_check(con))
            {
                return false;
            }
            // 쿼리문
            //SELECT User_Name, User_Password, User_PhoneNum FROM user_info WHERE User_Name='옥혜정' AND user_password='1234';
            string sqlcommend =
                string.Format(@"SELECT Name, Password, Nickname  FROM user_info WHERE Name='{0}' AND Password='{1}';", id, passwoard);

            MySqlCommand cmd = new MySqlCommand(sqlcommend, con);   // 쿼리문을 연결된 DB에 날리기 위한 객체
            reader = cmd.ExecuteReader();
            // reader가 읽은 데이터가 1개 이상 존재해?
            if (reader.HasRows)
            {
                // 읽은 데이터를 나열
                while (reader.Read())
                {
                    /*삼항연산자*/
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
                }//while문 끝
            }//if 끝
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

    // 회원가입
    // 1단계 : 아이디 , 비밀번호를 입력한다. 아이디 중복이 안될 시 2단계로 이동한다
    // 2단계 : 닉네임을 입력한다. 닉네임도 중복이 허용되지 않는다. 중복이 안되면 회원가입 완료
    public bool SignupStep1(string id, string password)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //아이디 중복 확인
            string sqlcheck =
                string.Format("SELECT COUNT(*) FROM user_info WHERE Name = '{0}';", id);
            MySqlCommand checkcmd = new MySqlCommand(sqlcheck, con);
            checkcmd.Parameters.AddWithValue("@id", id);

            object resultObj = checkcmd.ExecuteScalar();
            int count = Convert.ToInt32(resultObj);

            if(count > 0)   //아이디 중복
            {
                Debug.Log($"중복된 아이디 : {id}");
                return false;   
            }

            //INSERT INTO user_info VALUES("홍길동","1234","01000000000");
            string sqlsignup =
                 string.Format(@"INSERT INTO user_info VALUES('{0}','{1}', NULL)", id, password);
            MySqlCommand cmd = new MySqlCommand(sqlsignup, con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@password", password);
            
            // ExecuteNonQuery() : 영향을 받은 행의 수를 반환 
            // insert -> 1(1개의 행 추가)
            // update -> n(n개의 행 변경) 
            // delete -> n (n개의 행 삭제)
            int result = cmd.ExecuteNonQuery(); // 성공시 1 반환

            if (result > 0)
            {
                Debug.Log("다음 단계로 이동합니다");
                return true;
            }

            else
            {
                Debug.Log("실패");
                return false;
            }
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

            // 2단계 : 닉네임 추가
            string sqlnickname =
                string.Format(@"UPDATE user_info SET Nickname = '{0}' WHERE Name='{1}';", nickname, id);
            MySqlCommand cmd = new MySqlCommand(sqlnickname, con);

            int result = cmd.ExecuteNonQuery();

            if (result > 0)
            {
                Debug.Log("회원가입 성공");
                return true;
            }

            else
            {
                Debug.Log("회원가입 실패");
                return false;
            }
        }

        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }


    // 회원정보 수정
    public bool Updateinfo(string id, string curname, string newname, string newpwd)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //쿼리문
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='옥혜정';
            string sqlupdate =
                string.Format(@"UPDATE user_info SET Nickname = '{0}', Password='{1}' WHERE Nickname='{2}';", newname, newpwd,  curname);
            MySqlCommand cmd = new MySqlCommand(sqlupdate, con);

            int result = cmd.ExecuteNonQuery(); // 성공시 n개 반환

            if (result > 0)
            {
                info = new User_info(id, newpwd, newname);
                Debug.Log("회원정보 수정 완료");
                return true;
            }

            else
            {
                Debug.Log("수정할 수 없습니다.");
                return false;
            }
        }

        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
    }

    // 회원탈퇴
    public bool Deleteinfo(string id, string password)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //쿼리문
            //DELETE FROM user_info WHERE User_Name='옥혜정';
            string deletesql =
                string.Format(@"DELETE FROM user_info WHERE Name='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(deletesql, con);

            int result = cmd.ExecuteNonQuery(); //삭제한 n개 수

            if (result > 0)
            {
                Debug.Log($"{id}가 삭제되었습니다.");
                info = null;
                return true;
            }

            else
            {
                Debug.Log("삭제 실패했습니다.");
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

