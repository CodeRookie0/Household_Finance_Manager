using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Main.Logic
{
    public static class Service
    {
        // Validation Methods
        public static bool IsEmailExistis(string email)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@Email", email));
                return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;
            }
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, pattern);
        }

        public static bool ValidatePassword(string password)
        {
            string summaryMessage = "Hasło";
            bool passedValidation = true;

            if (password.Length < 8 || password.Length > 15)
            {
                summaryMessage = summaryMessage + "\n- długość musi wynosić od 8 do 15 znaków. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsUpper))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną wielką literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsLower))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną małą literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsDigit))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną cyfrę. ";
                passedValidation = false;
            }

            string specialCharacters = "-_!*#$&";
            if (!password.Any(c => specialCharacters.Contains(c)))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jeden znak specjalny (-, _, !, *, #, $, &). ";
                passedValidation = false;
            }

            if (!passedValidation)
            {
                MessageBox.Show(summaryMessage, "Błąd walidacji hasła", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return passedValidation;
        }

        public static bool ValidateUserPassword(int userId, string password)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT PasswordHash,Salt FROM Users WHERE UserID=@UserID";
                var result = database.ExecuteQuery(selectQuery, new Microsoft.Data.Sqlite.SqliteParameter("@UserID", userId));
                if (result.Rows.Count > 0)
                {
                    var tmp = result.Rows[0];

                    string hashPassword = HashPassword(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(tmp["Salt"].ToString()));

                    if (hashPassword == tmp["PasswordHash"].ToString())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static bool IsPrimaryUser(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                int familyId = GetFamilyIdByPrimaryUserId(userId);
                string selectQuery = "SELECT COUNT(1) FROM Family WHERE PrimaryUserID = @UserID AND FamilyID = @FamilyId";
                var result = database.ExecuteScalar(selectQuery,
                    new SqliteParameter("@UserID", userId),
                    new SqliteParameter("@FamilyId", familyId));

                return Convert.ToInt32(result) > 0;
            }
        }

        static public bool IsFamilyCodeInUse(string code)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(1) FROM Family WHERE FamilyCode = @FamilyCode";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyCode", code));
                return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;
            }
        }

        public static bool UserHasFamily(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyID FROM Users WHERE UserID=@UserID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@UserID", userId));
                if (result.Rows.Count > 0)
                {
                    return result.Rows[0]["FamilyID"] != DBNull.Value;
                }
                return false;
            }
        }

        public static bool HasPendingJoinRequest(int userId,int familyId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM JoinRequests WHERE UserID = @UserID AND FamilyID = @FamilyID AND RequestStatusID = 1";
                var result = database.ExecuteScalar(selectQuery,
                    new SqliteParameter("@UserID", userId),
                    new SqliteParameter("@FamilyID", familyId));

                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        static public string HashPassword(byte[] bytesToHash, byte[] salt)
        {
            var byteResult = new Rfc2898DeriveBytes(bytesToHash, salt, 1000);
            return Convert.ToBase64String(byteResult.GetBytes(24));
        }

        static public string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
        }

        public static bool LoginUser(string UserName, string Password, ref int userId) //Poprawić generowanie hasha i soli [WSTĘPNA WERSJA]
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT UserID,PasswordHash,Salt FROM Users WHERE Email=@InputUserName";
                var result = database.ExecuteQuery(selectQuery, new Microsoft.Data.Sqlite.SqliteParameter("@InputUserName", UserName));
                if (result.Rows.Count > 0)
                {
                    var tmp = result.Rows[0];

                    string hashPassword = HashPassword(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes(tmp["Salt"].ToString()));

                    if (hashPassword == tmp["PasswordHash"].ToString())
                    {
                        userId = Int32.Parse(tmp["UserID"].ToString());
                        return true;
                    }
                }
                return false;
            }

        }

        public static bool LeaveFamily(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateUserQuery = "UPDATE Users SET FamilyID = NULL WHERE UserID = @UserID";

                try
                {
                    database.ExecuteNonQuery(updateUserQuery, new SqliteParameter("@UserID", userId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        // Add Methods
        public static bool AddUser(string userName, string email, string password, string Salt)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO Users (UserName, Email,PasswordHash,Salt,RoleId,CreatedAt) VALUES (@name, @email, @password,@salt, @roleId,@createdAt)";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@name", userName),
                        new SqliteParameter("@email", email),
                        new SqliteParameter("@password", password),
                        new SqliteParameter("@salt", Salt),
                        new SqliteParameter("@roleId", 1),
                        new SqliteParameter("@createdAt", DateTime.Now.ToString()));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool AddFamily(int userId,string familyName, string familyCode)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO Family (FamilyName, FamilyCode, PrimaryUserID, CreatedAt) VALUES (@FamilyName, @FamilyCode, @PrimaryUserID, @CreatedAt); SELECT last_insert_rowid();";

                try
                {
                    var familyId = database.ExecuteScalar(insertQuery,
                        new SqliteParameter("@FamilyName", familyName),
                        new SqliteParameter("@FamilyCode", familyCode),
                        new SqliteParameter("@PrimaryUserID", userId),
                        new SqliteParameter("@CreatedAt", DateTime.Now.ToString()));

                    string updateUserQuery = "UPDATE Users SET FamilyID = @FamilyID WHERE UserID = @UserID";
                    
                    database.ExecuteNonQuery(updateUserQuery,
                        new SqliteParameter("@FamilyID", familyId),
                        new SqliteParameter("@UserID", userId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool AddJoinRequest(int familyId, int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO JoinRequests (FamilyID, UserID, RequestStatusID) VALUES (@FamilyID, @UserID, 1)";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@FamilyID", familyId),
                        new SqliteParameter("@UserID", userId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        // Update Methods

        public static bool UpdateFamilyName(int familyId, string newFamilyName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateQuery = "UPDATE Family SET FamilyName = @FamilyName WHERE FamilyID = @FamilyID";
                try
                {
                    database.ExecuteNonQuery(updateQuery,
                        new SqliteParameter("@FamilyName", newFamilyName),
                        new SqliteParameter("@FamilyID", familyId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // Delete Methods
        public static bool DeleteFamily(int familyId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                using (var transaction = database.BeginTransaction())
                {
                    try
                    {
                        string updateUsersQuery = "UPDATE Users SET FamilyID = NULL WHERE FamilyID = @FamilyId";
                        database.ExecuteNonQuery(updateUsersQuery, new SqliteParameter("@FamilyId", familyId));

                        string deleteFamilyQuery = "DELETE FROM Family WHERE FamilyID = @FamilyID";
                        database.ExecuteNonQuery(deleteFamilyQuery, new SqliteParameter("@FamilyID", familyId));

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // GET Methods
        public static string GetCodeByFamilyId(int familyId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyCode FROM Family WHERE FamilyID = @FamilyID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyID", familyId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["FamilyCode"].ToString();
            }
        }
        public static string GetFamilyNameByFamilyId(int familyId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyName FROM Family WHERE FamilyID = @FamilyID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyID", familyId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["FamilyName"].ToString();
            }
        }
        public static string GetFamilyCreatedAtByFamilyId(int familyId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT CreatedAt FROM Family WHERE FamilyID = @FamilyID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyID", familyId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return Convert.ToDateTime(result.Rows[0]["CreatedAt"]).ToString("yyyy-MM-dd");
            }
        }

        public static int GetFamilyIdByPrimaryUserId(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyID FROM Family WHERE PrimaryUserID = @PrimaryUserID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@PrimaryUserID", userId));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                return Convert.ToInt32(result.Rows[0]["FamilyID"]);
            }
        }

        public static int GetFamilyIdByCode(string familyCode)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyID FROM Family WHERE FamilyCode = @FamilyCode";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyCode", familyCode));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                return Convert.ToInt32(result.Rows[0]["FamilyID"]);
            }
        }

        public static List<JoinRequest> GetJoinRequestsByUserId(int userId)
        {
            List<JoinRequest> joinRequests = new List<JoinRequest>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT JoinRequests.JoinRequestID, Family.FamilyCode, " +
                               "JoinRequests.JoinRequestDate, RequestStatuses.RequestStatusName " +
                               "FROM JoinRequests " +
                               "INNER JOIN Family ON JoinRequests.FamilyID = Family.FamilyID " +
                               "INNER JOIN RequestStatuses ON JoinRequests.RequestStatusID = RequestStatuses.RequestStatusID " +
                               "WHERE JoinRequests.UserID = @UserID";

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@UserID", userId));

                foreach (DataRow row in result.Rows)
                {
                    JoinRequest request = new JoinRequest
                    {
                        JoinRequestID = Convert.ToInt32(row["JoinRequestID"]),
                        FamilyCode = row["FamilyCode"].ToString(),
                        JoinRequestDate = DateTime.Parse(row["JoinRequestDate"].ToString()),
                        RequestStatus = row["RequestStatusName"].ToString()
                    };
                    joinRequests.Add(request);
                }
            }

            return joinRequests;
        }

    }
}
