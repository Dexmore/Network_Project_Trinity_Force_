using System;
using System.IO;
using UnityEngine;
using MySql.Data.MySqlClient;
using LitJson;

public class User_info
{
    public string User_name     { get; private set; }
    public string User_Password { get; set; }
    public string User_Nickname { get; set; }

    public User_info(string name, string password, string nickname)
    {
        User_name     = name;
        User_Password = password;
        User_Nickname = nickname;
    }
}

public class SQLManager : MonoBehaviour
{
    public static SQLManager instance { get; private set; }
    public User_info info { get; private set; }

    private MySqlConnection con;
    private string dbFolder;
    private string jsonPath;

    void Awake()
    {
        // 싱글톤
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

        // Database 폴더/파일 체크
        dbFolder = Path.Combine(Application.dataPath, "Database");
        if (!Directory.Exists(dbFolder))
            Directory.CreateDirectory(dbFolder);

        jsonPath = Path.Combine(dbFolder, "LoginJson.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"LoginJson.json 파일을 찾을 수 없습니다: {jsonPath}");
            return;
        }

        // 접속 문자열 읽기
        string json = File.ReadAllText(jsonPath);
        JsonData jd  = JsonMapper.ToObject(json);
        string connStr =
            $"Server={jd[0]["IP"]};" +
            $"Database={jd[0]["TableName"]};" +
            $"Uid={jd[0]["ID"]};" +
            $"Pwd={jd[0]["PW"]};" +
            $"Port={jd[0]["PORT"]};" +
            "Charset=utf8;";

        try
        {
            con = new MySqlConnection(connStr);
            con.Open();
            Debug.Log("SQL server Open complete!");
        }
        catch (Exception e)
        {
            Debug.LogError($"SQL connection error: {e.Message}");
        }
    }

    // 연결 상태 보장
    private bool EnsureConnection()
    {
        if (con == null) return false;
        if (con.State != System.Data.ConnectionState.Open)
            con.Open();
        return con.State == System.Data.ConnectionState.Open;
    }

    #region 로그인
    public bool Login(string id, string password)
    {
        if (!EnsureConnection()) return false;

        const string sql = @"
            SELECT Name, Password, Nickname
            FROM user_info
            WHERE Name = @id AND Password = @pw;
        ";

        try
        {
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pw", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        string pwd  = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string nick = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pwd))
                            return false;

                        info = new User_info(name, pwd, nick);
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Login error: {e.Message}");
        }
        return false;
    }
    #endregion

    #region 로그인 중 닉네임 설정
    public bool CompleteLoginwithName(string id, string newNickname)
    {
        if (!EnsureConnection()) return false;

        // 1) 중복 검사
        const string checkSql = @"SELECT COUNT(*) FROM user_info WHERE Nickname = @nick;";
        try
        {
            using (var checkCmd = new MySqlCommand(checkSql, con))
            {
                checkCmd.Parameters.AddWithValue("@nick", newNickname);
                var cnt = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (cnt > 0) return false;
            }

            // 2) 닉네임 업데이트
            const string updSql = @"
                UPDATE user_info
                SET Nickname = @nick
                WHERE Name = @id;
            ";
            using (var updCmd = new MySqlCommand(updSql, con))
            {
                updCmd.Parameters.AddWithValue("@nick", newNickname);
                updCmd.Parameters.AddWithValue("@id",   id);
                if (updCmd.ExecuteNonQuery() == 0)
                    return false;
            }

            // 3) 정보 리로딩
            const string fetchSql = @"
                SELECT Name, Password, Nickname
                FROM user_info
                WHERE Name = @id;
            ";
            using (var fetchCmd = new MySqlCommand(fetchSql, con))
            {
                fetchCmd.Parameters.AddWithValue("@id", id);
                using (var reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        string pwd  = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string nick = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        info = new User_info(name, pwd, nick);
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"CompleteLoginwithName error: {e.Message}");
        }
        return false;
    }
    #endregion

    #region 회원가입
    public bool SignupStep1(string id, string password)
    {
        if (!EnsureConnection()) return false;

        const string checkSql = @"SELECT Nickname FROM user_info WHERE Name = @id;";
        try
        {
            using (var cmd = new MySqlCommand(checkSql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();

                if(result != null)
                {
                    if (result == DBNull.Value)
                    {
                        // 아이디는 있지만 닉네임이 없을 때
                        return true;
                    }
                    else
                    {
                        //아이디 + 닉네임 모두 있음 -> 중복된 아이디
                        return false;
                    }
                }


                // 1단계 : 아이디, 비밀번호 입력 -> DB에 Insert
                const string insSql = @"
                        INSERT INTO user_info (Name, Password, Nickname)
                        VALUES (@id, @pw, NULL);
                    ";
                    using (var ins = new MySqlCommand(insSql, con))
                    {
                        ins.Parameters.AddWithValue("@id", id);
                        ins.Parameters.AddWithValue("@pw", password);
                        return ins.ExecuteNonQuery() > 0;
                    }
                
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SignupStep1 error: {e.Message}");
        }
        return false;
    }

    public bool SignupStep2(string id, string newNickname)
    {
        if (!EnsureConnection()) return false;

        const string checkSql = @"SELECT COUNT(*) FROM user_info WHERE Nickname = @nick AND Name != @id;";
        try
        {
            using (var cmd = new MySqlCommand(checkSql, con))
            {
                cmd.Parameters.AddWithValue("@nick", newNickname);
                cmd.Parameters.AddWithValue("@id", id);
                var cnt = Convert.ToInt64(cmd.ExecuteScalar());
                if (cnt > 0) return false;
            }

            const string updSql = @"
                UPDATE user_info
                SET Nickname = @nick
                WHERE Name = @id;
            ";
            using (var upd = new MySqlCommand(updSql, con))
            {
                upd.Parameters.AddWithValue("@nick", newNickname);
                upd.Parameters.AddWithValue("@id",   id);
                return upd.ExecuteNonQuery() > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SignupStep2 error: {e.Message}");
        }
        return false;
    }
    #endregion

    #region 닉네임 변경
    public bool UpdateNicknameinfo(string id, string currentName, string newName)
    {
        if (!EnsureConnection()) return false;

        // 1) 중복 검사
        const string chk = @"SELECT COUNT(*) FROM user_info WHERE Nickname = @new;";
        try
        {
            using (var cmd = new MySqlCommand(chk, con))
            {
                cmd.Parameters.AddWithValue("@new", newName);
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0)
                    return false;
            }

            // 2) 업데이트
            const string upd = @"
                UPDATE user_info
                SET Nickname = @new
                WHERE Name = @id;
            ";
            using (var cmd = new MySqlCommand(upd, con))
            {
                cmd.Parameters.AddWithValue("@new", newName);
                cmd.Parameters.AddWithValue("@id",  id);
                if (cmd.ExecuteNonQuery() == 0) return false;
            }

            // 3) 메모리 값 갱신
            info.User_Nickname = newName;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"UpdateNicknameinfo error: {e.Message}");
            return false;
        }
    }
    #endregion

    #region 비밀번호 변경
    public bool Updatepasswordinfo(string id, string newPassword)
    {
        if (!EnsureConnection()) return false;

        const string upd = @"
            UPDATE user_info
            SET Password = @pw
            WHERE Name = @id;
        ";
        try
        {
            using (var cmd = new MySqlCommand(upd, con))
            {
                cmd.Parameters.AddWithValue("@pw", newPassword);
                cmd.Parameters.AddWithValue("@id", id);
                if (cmd.ExecuteNonQuery() == 0) return false;
            }

            info.User_Password = newPassword;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Updatepasswordinfo error: {e.Message}");
            return false;
        }
    }
    #endregion

    #region 회원 탈퇴
    public bool Deleteinfo(string id, string password, string nickname)
    {
        if (!EnsureConnection()) return false;

        const string del = @"
            DELETE FROM user_info
            WHERE Name = @id
              AND Password = @pw
              AND Nickname = @nick;
        ";
        try
        {
            using (var cmd = new MySqlCommand(del, con))
            {
                cmd.Parameters.AddWithValue("@id",   id);
                cmd.Parameters.AddWithValue("@pw",   password);
                cmd.Parameters.AddWithValue("@nick", nickname);

                if (cmd.ExecuteNonQuery() == 0) return false;
            }

            // 탈퇴 성공 시 메모리 초기화
            info = null;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Deleteinfo error: {e.Message}");
            return false;
        }
    }
    #endregion
}
