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

            //1. 아이디 중복 확인
            // 닉네임이 nulll인지 아닌지  -> 가입 여부
            string sqlcheck =
                string.Format(@"SELECT Nickname FROM user_info WHERE Name = '{0}';", id);
            MySqlCommand checkcmd = new MySqlCommand(sqlcheck, con);

            //ExecuteScalar() : 결과로 반환된 첫 번째 행의 첫 번째 열 값을 반환하는 메소드 -> 값 1개만 가져올 때 쓴다
            // 성능이 빨라 불필요한 데이터를 가져오지 않음
            // ExecuteScalar()는 반환되는 값이 object
            object resultObj = checkcmd.ExecuteScalar();

            //2.  아이디가 아예 없다면? 새로 Insert
            //INSERT INTO user_info VALUES("홍길동","1234","01000000000");
            if(resultObj == null)  
            {
                string sqlsignup =
                     string.Format(@"INSERT INTO user_info VALUES('{0}','{1}', NULL)", id, password);
                MySqlCommand cmd = new MySqlCommand(sqlsignup, con);
            
                // ExecuteNonQuery() : 영향을 받은 행의 수를 반환 (결과를 반환하지 않는 쿼리에 사용)
                // insert -> 1(1개의 행 추가)
                // update -> n(n개의 행 변경) 
                // delete -> n (n개의 행 삭제)
                int result = cmd.ExecuteNonQuery(); // 성공시 1 반환
                
                if(result > 0)  // 아이디 등록 완료
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }

            //3. 아이디는 있는데 Nickname이 null -> 재가입으로 간주, 비번만 업데이트 가능
            // DBNull.Value은 SQL에서 NULL
            if (resultObj is DBNull)  //is는 타입 검사
            {
                string sqlupdate =
                    string.Format(@"UPDATE user_info SET Password = '{0}' WHERE Name = '{1}';", password, id);
                MySqlCommand cmdupdate = new MySqlCommand(sqlupdate, con);
                int updateresult = cmdupdate.ExecuteNonQuery();
                return updateresult > 0;
            }

            //4. 아이디 있고, 닉네임 있다 -> 중복으로 간주
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

            // 닉네임 중복 확인
            string sqlcheckname =
                string.Format(@"SELECT COUNT(*) FROM user_info WHERE Nickname = '{0}';", nickname);
            MySqlCommand cmdcheck = new MySqlCommand(sqlcheckname, con);
            object resultObj = cmdcheck.ExecuteScalar();
            int count = Convert.ToInt32(resultObj);

            if(count > 0)   // 닉네임 중복
            {
                return false;
            }


            // 2단계 : 닉네임 추가
            string sqlnickname =
                string.Format(@"UPDATE user_info SET Nickname = '{0}' WHERE Name='{1}';", nickname, id);
            MySqlCommand cmd = new MySqlCommand(sqlnickname, con);

            int result = cmd.ExecuteNonQuery();

            if (result > 0) // 회원가입 성공
            {
                return true;
            }

            else //회원 가입 실패
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


    // 회원정보 수정
    // 1단계 : 닉네임과 비밀번호 중 한 개를 선택한다.
    // 2단계 : 닉네임 선택하면 닉네임만 변경, 비밀번호를 선택하면 비밀번호만 변경

    // 닉네임 변경 함수
    public bool UpdateNicknameinfo(string id, string currentname, string newname)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }
            //닉네임 중복 확인
            string sqlcheckname =
                string.Format(@"SELECT COUNT(*) FROM user_info WHERE Nickname = '{0}';", newname);
            MySqlCommand checkcmd = new MySqlCommand(sqlcheckname, con);
            object resultObj = checkcmd.ExecuteScalar();
            int count = Convert.ToInt32(resultObj);

            if(count > 0)   // 닉네임 중복 시
            {
                return false;
            }

            //쿼리문
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='옥혜정';
            string sqlupdatename =
                string.Format(@"UPDATE user_info SET Nickname = '{0}' WHERE Name ='{1}';", newname, id);
            MySqlCommand cmd = new MySqlCommand(sqlupdatename, con);

            int result = cmd.ExecuteNonQuery(); // 성공시 n개 반환

            if (result > 0) // 닉네임 변경 완료
            {
                info.User_Nickname = newname;
                return true;
            }

            else //변경 실패
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

    // 비밀번호 변경
    public bool Updatepasswordinfo(string id, string newpassword)
    {
        try
        {
            if (!connection_check(con))
            {
                return false;
            }

            //쿼리문
            //UPDATE user_info SET User_Password='4696' WHERE User_Name='옥혜정';
            string sqlupdatepwd =
                string.Format(@"UPDATE user_info SET Password = '{0}' WHERE Name ='{1}';", newpassword, id);
            MySqlCommand cmd = new MySqlCommand(sqlupdatepwd, con);

            int result = cmd.ExecuteNonQuery(); // 성공시 n개 반환

            if (result > 0) //비밀번호 변경 완료
            {
                info.User_Password = newpassword;
                return true;
            }

            else // 변경실패
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

