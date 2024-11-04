using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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

        static public bool IsFamilyCodeInUse(string code)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(1) FROM Family WHERE FamilyCode = @FamilyCode";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FamilyCode", code));
                return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;
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

        public static bool LoginUser(string UserName, string Password) //Poprawić generowanie hasha i soli [WSTĘPNA WERSJA]
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT PasswordHash,Salt FROM Users WHERE Email=@InputUserName";
                var result = database.ExecuteQuery(selectQuery, new Microsoft.Data.Sqlite.SqliteParameter("@InputUserName", UserName));
                if (result.Rows.Count > 0)
                {
                    var tmp = result.Rows[0];

                    string hashPassword = HashPassword(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes(tmp["Salt"].ToString()));

                    if (hashPassword == tmp["PasswordHash"].ToString())
                    {
                        return true;
                    }
                }

                return false;
            }

        }

        // Add Methods
        public static bool AddUser(string username, string email, string password, string Salt)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO Users (UserName, Email,PasswordHash,Salt,RoleId,CreatedAt) VALUES (@name, @email, @password,@salt, @roleId,@createdAt)";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@name", username),
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


        // Update Methods


        // Delete Methods
    }
}
