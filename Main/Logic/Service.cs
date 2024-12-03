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
using Main.Models;
using System.Runtime.InteropServices.ComTypes;

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

        public static bool IsCategoryFavoriteForUser(int userId, int categoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM FavoriteCategories WHERE UserID = @UserID AND CategoryID = @CategoryID";
                var result = database.ExecuteQuery(selectQuery,
                        new SqliteParameter("@CategoryID", categoryId),
                        new SqliteParameter("@UserID", userId));
                int count = Convert.ToInt32(result.Rows[0][0]);
                return count > 0;
            }
        }

        public static bool IsCategoryPresentInDefaultOrFamily(int userId, string categoryName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM Categories WHERE UserID IS NULL AND CategoryName = @CategoryName";
                var result = database.ExecuteQuery(selectQuery,
                        new SqliteParameter("@CategoryName", categoryName));
                int count = Convert.ToInt32(result.Rows[0][0]);
                if (count > 0)
                {
                    return true;
                }
            }

            int familyId = GetFamilyIdByMemberId(userId);
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM Categories WHERE UserID = @UserID AND CategoryName = @CategoryName";
                foreach (var familyMember in familyMembers)
                {
                    var result = database.ExecuteQuery(selectQuery,
                            new SqliteParameter("@CategoryName", categoryName),
                            new SqliteParameter("@UserID", familyMember.UserID));
                    int count = Convert.ToInt32(result.Rows[0][0]);

                    if (count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsSubcategoryPresentInDefaultOrFamily(int userId, string categoryName, string subcategoryName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string getCategoryIdQuery = "SELECT CategoryID FROM Categories WHERE UserID IS NULL AND CategoryName = @CategoryName";
                var categoryResult = database.ExecuteQuery(getCategoryIdQuery,
                        new SqliteParameter("@CategoryName", categoryName));

                if (categoryResult.Rows.Count > 0)
                {
                    int categoryId = Convert.ToInt32(categoryResult.Rows[0]["CategoryID"]);

                    string subcategoryQuery = "SELECT COUNT(*) FROM Subcategories WHERE CategoryID = @CategoryID AND SubcategoryName = @SubcategoryName";
                    var subcategoryResult = database.ExecuteQuery(subcategoryQuery,
                            new SqliteParameter("@CategoryID", categoryId),
                            new SqliteParameter("@SubcategoryName", subcategoryName));
                    int subcategoryCount = Convert.ToInt32(subcategoryResult.Rows[0][0]);
                    return subcategoryCount > 0;
                }
            }

            int familyId = GetFamilyIdByMemberId(userId);
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM Categories WHERE UserID = @UserID AND CategoryName = @CategoryName";
                foreach (var familyMember in familyMembers)
                {
                    var result = database.ExecuteQuery(selectQuery,
                            new SqliteParameter("@CategoryName", categoryName),
                            new SqliteParameter("@UserID", familyMember.UserID));
                    int count = Convert.ToInt32(result.Rows[0][0]);

                    if (count > 0)
                    {
                        string getCategoryIdQueryFamily = "SELECT CategoryID FROM Categories WHERE UserID = @UserID AND CategoryName = @CategoryName";
                        var categoryResultFamily = database.ExecuteQuery(getCategoryIdQueryFamily,
                                new SqliteParameter("@CategoryName", categoryName),
                                new SqliteParameter("@UserID", familyMember.UserID));

                        if (categoryResultFamily.Rows.Count > 0)
                        {
                            int categoryId = Convert.ToInt32(categoryResultFamily.Rows[0]["CategoryID"]);

                            string subcategoryQuery = "SELECT COUNT(*) FROM Subcategories WHERE CategoryID = @CategoryID AND SubcategoryName = @SubcategoryName";
                            var subcategoryResult = database.ExecuteQuery(subcategoryQuery,
                                    new SqliteParameter("@CategoryID", categoryId),
                                    new SqliteParameter("@SubcategoryName", subcategoryName));
                            int subcategoryCount = Convert.ToInt32(subcategoryResult.Rows[0][0]);
                            if (subcategoryCount > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsChildTransaction(int transactionID)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT u.RoleID FROM Transactions t INNER JOIN Users u ON t.UserID = u.UserID WHERE t.TransactionID = @TransactionID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@TransactionID", transactionID));
                if (result != null && result.Rows.Count > 0)
                {
                    int roleID = Convert.ToInt32(result.Rows[0]["RoleID"]);
                    return roleID == 3;
                }

                return false;
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
        public static int AddCategory(string categoryName,int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                int categoryId = -1;

                string insertQuery = "INSERT INTO Categories (CategoryName, UserID) VALUES (@CategoryName, @UserID);";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@CategoryName", categoryName),
                        new SqliteParameter("@UserID", userId));

                    string selectQuery = "SELECT CategoryID FROM Categories WHERE CategoryName = @CategoryName AND UserID = @UserID ORDER BY CategoryID DESC LIMIT 1;";

                    var result = database.ExecuteQuery(selectQuery,
                        new SqliteParameter("@CategoryName", categoryName),
                        new SqliteParameter("@UserID", userId));

                    if (result != null && result.Rows.Count > 0)
                    {
                        categoryId = Convert.ToInt32(result.Rows[0]["CategoryID"]);
                    }
                }
                catch (Exception ex)
                {
                    return -1;
                }
                return categoryId;
            }
        }

        public static bool AddSubcategory(int categoryId, string subcategoryName, int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO Subcategories (CategoryID, SubcategoryName, UserID) VALUES (@CategoryID, @SubcategoryName, @UserID);";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@CategoryID", categoryId),
                        new SqliteParameter("@SubcategoryName", subcategoryName),
                        new SqliteParameter("@UserID", userId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool AddCategoryToFavorites(int userId, int categoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = "INSERT INTO FavoriteCategories (UserID, CategoryID) VALUES (@UserID, @CategoryID);";

                try
                {
                    database.ExecuteNonQuery(insertQuery,
                        new SqliteParameter("@CategoryID", categoryId),
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
        public static bool UpdateCategoryName(int categoryId, string newCategoryName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateQuery = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID";
                try
                {
                    database.ExecuteNonQuery(updateQuery,
                        new SqliteParameter("@CategoryName", newCategoryName),
                        new SqliteParameter("@CategoryID", categoryId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public static bool UpdateSubcategoryName(int subcategoryId, string newSubcategoryName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateQuery = "UPDATE Subcategories SET SubcategoryName = @SubcategoryName WHERE SubcategoryID = @SubcategoryID";
                try
                {
                    database.ExecuteNonQuery(updateQuery,
                        new SqliteParameter("@SubcategoryName", newSubcategoryName),
                        new SqliteParameter("@SubcategoryID", subcategoryId));
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
                try
                {
                    string updateUsersQuery = "UPDATE Users SET FamilyID = NULL WHERE FamilyID = @FamilyId";
                    database.ExecuteNonQuery(updateUsersQuery, new SqliteParameter("@FamilyId", familyId));

                    string deleteFamilyQuery = "DELETE FROM Family WHERE FamilyID = @FamilyID";
                    database.ExecuteNonQuery(deleteFamilyQuery, new SqliteParameter("@FamilyID", familyId));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public static bool DeleteCategory(int categoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string updateTransactionsQuery = "UPDATE Transactions SET CategoryID = NULL, SubcategoryID = NULL WHERE CategoryID = @CategoryId OR SubcategoryID IN (SELECT SubcategoryID FROM Subcategories WHERE CategoryID = @CategoryId)";
                    database.ExecuteNonQuery(updateTransactionsQuery, new SqliteParameter("@CategoryId", categoryId));

                    string deleteSubcategoriesQuery = "DELETE FROM Subcategories WHERE CategoryID = @CategoryId";
                    database.ExecuteNonQuery(deleteSubcategoriesQuery, new SqliteParameter("@CategoryId", categoryId));

                    string deleteCategoriesQuery = "DELETE FROM Categories WHERE CategoryID = @CategoryId";
                    database.ExecuteNonQuery(deleteCategoriesQuery, new SqliteParameter("@CategoryId", categoryId));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public static bool DeleteSubcategory(int subcategoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string updateTransactionsQuery = "UPDATE Transactions SET SubcategoryID = NULL WHERE SubcategoryID = @SubcategoryID";
                    database.ExecuteNonQuery(updateTransactionsQuery, new SqliteParameter("@SubcategoryID", subcategoryId));

                    string deleteSubcategoriesQuery = "DELETE FROM Subcategories WHERE SubcategoryID = @SubcategoryID";
                    database.ExecuteNonQuery(deleteSubcategoriesQuery, new SqliteParameter("@SubcategoryID", subcategoryId));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool DeleteCategoryFromFavorites(int userId, int categoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string deleteFavoriteCategoryQuery = "DELETE FROM FavoriteCategories WHERE UserID = @UserID AND CategoryID = @CategoryID";
                    database.ExecuteNonQuery(deleteFavoriteCategoryQuery, new SqliteParameter("@UserID", userId), new SqliteParameter("@CategoryID", categoryId));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // GET Methods

        public static string GetUserNameByUserID(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT UserName FROM Users WHERE UserID = @UserID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@UserID", userId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["UserName"].ToString();
            }
        }

        public static int GetRoleIDByUserID(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT RoleID FROM Users WHERE UserID = @UserID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@UserID", userId));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                return result.Rows[0]["RoleID"] == DBNull.Value ? -1 : Convert.ToInt32(result.Rows[0]["RoleID"]);
            }
        }

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

        public static int GetFamilyIdByMemberId(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FamilyID FROM Users WHERE UserID = @MemberID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@MemberID", userId));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                var familyId = result.Rows[0]["FamilyID"];
                if (familyId == DBNull.Value)
                {
                    return -1;
                }

                return Convert.ToInt32(familyId);
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
                string query = "SELECT JoinRequests.JoinRequestID, Family.FamilyName, " +
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
                        FamilyName = row["FamilyName"].ToString(),
                        JoinRequestDate = DateTime.Parse(row["JoinRequestDate"].ToString()),
                        RequestStatus = row["RequestStatusName"].ToString()
                    };
                    joinRequests.Add(request);
                }
            }
            return joinRequests;
        }

        public static List<Category> GetUserCategories(int userId)
        {
            List<Category> categories = new List<Category>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT Categories.CategoryID, Categories.CategoryName, Categories.UserID " +
                               "FROM Categories " +
                               "WHERE Categories.UserID = @UserID";

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@UserID", userId));

                foreach (DataRow row in result.Rows)
                {
                    Category category = new Category
                    {
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CategoryName = row["CategoryName"].ToString(),
                        UserID = row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"])
                    };
                    categories.Add(category);
                }
            }
            return categories;
        }

        public static List<Category> GetDefaultCategories()
        {
            List<Category> categories = new List<Category>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT Categories.CategoryID, Categories.CategoryName, Categories.UserID " +
                               "FROM Categories " +
                               "WHERE Categories.UserID IS NULL";

                DataTable result = database.ExecuteQuery(query);

                foreach (DataRow row in result.Rows)
                {
                    Category category = new Category
                    {
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        CategoryName = row["CategoryName"].ToString(),
                        UserID = row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"])
                    };
                    categories.Add(category);
                }
            }
            return categories;
        }

        public static List<Category> GetFamilyCategories(int familyId)
        {
            List<Category> familyCategories = new List<Category>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;
                List<Category> userCategories = GetUserCategories(userId);

                foreach (Category category in userCategories)
                {
                    if (!familyCategories.Any(s => s.CategoryID == category.CategoryID))
                    {
                        familyCategories.Add(category);
                    }
                }
            }
            return familyCategories;
        }

        public static List<Subcategory> GetSubcategoriesByCategoryId(int categoryId)
        {
            List<Subcategory> subcategories = new List<Subcategory>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT Subcategories.SubcategoryID, Subcategories.SubcategoryName, " +
                               "Subcategories.CategoryID, Subcategories.UserID " +
                               "FROM Subcategories " +
                               "WHERE Subcategories.CategoryID=@CategoryID";

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@CategoryID", categoryId));

                foreach (DataRow row in result.Rows)
                {
                    Subcategory subcategory = new Subcategory
                    {
                        SubcategoryID = Convert.ToInt32(row["SubcategoryID"]),
                        SubcategoryName = row["SubcategoryName"].ToString(),
                        CategoryID = Convert.ToInt32(row["CategoryID"]),
                        UserID = row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"])
                    };
                    subcategories.Add(subcategory);
                }
            }
            return subcategories;
        }

        public static string GetCategoryNameByCategoryID(int categoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT CategoryName FROM Categories WHERE CategoryID = @CategoryID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@CategoryID", categoryId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["CategoryName"].ToString();
            }
        }

        public static string GetSubcategoryNameBySubcategoryID(int subcategoryId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT SubcategoryName FROM Subcategories WHERE SubcategoryID = @SubcategoryID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@SubcategoryID", subcategoryId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["SubcategoryName"].ToString();
            }
        }

        public static string GetStoreNameByStoreID(int storeId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT StoreName FROM Stores WHERE StoreID = @StoreID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@StoreID", storeId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["StoreName"].ToString();
            }
        }

        public static ObservableCollection<Transaction> GetUserTransactions(int userId)
        {
            ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT TransactionID, UserID, Amount, TransactionTypeID, CategoryID, SubcategoryID, StoreID, Note, Date FROM Transactions WHERE UserID = @UserId";

                SqliteParameter userIdParam = new SqliteParameter("@UserId", userId);
                DataTable result = database.ExecuteQuery(query, userIdParam);

                foreach (DataRow row in result.Rows)
                {
                    Transaction transaction = new Transaction
                    {
                        TransactionID = row["TransactionID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionID"]),
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        Amount = row["Amount"] == DBNull.Value ? 0.0m : Convert.ToDecimal(row["Amount"]), 
                        TransactionTypeID = row["TransactionTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionTypeID"]),
                        CategoryID = row["CategoryID"] == DBNull.Value ? -1 : Convert.ToInt32(row["CategoryID"]),
                        SubcategoryID = row["SubcategoryID"] == DBNull.Value ? -1 : Convert.ToInt32(row["SubcategoryID"]),
                        StoreID = row["StoreID"] == DBNull.Value ? -1 : Convert.ToInt32(row["StoreID"]),
                        Note = row["Note"] == DBNull.Value ? null : Convert.ToString(row["Note"]),
                        Date = row["Date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["Date"])
                    };
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }

        public static List<Transaction> GetFilteredUserTransactions(int? userId = null,DateTime? dateFrom = null,DateTime? dateTo = null,int? categoryId = null,int? storeId = null,int? transactionTypeId = null,double? amountFrom = null,double? amountTo = null)
        {
            List<Transaction> transactions = new List<Transaction>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT TransactionID, UserID, Amount, TransactionTypeID, CategoryID, SubcategoryID, StoreID, Note, Date FROM Transactions WHERE 1 = 1";
                List<SqliteParameter> parameters = new List<SqliteParameter>();

                if (userId.HasValue)
                {
                    query += " AND UserID = @UserId";
                    parameters.Add(new SqliteParameter("@UserId", userId.Value));
                }
                if (dateFrom.HasValue)
                {
                    DateTime startDate = dateFrom.Value.Date;
                    query += " AND Date >= @DateFrom";
                    parameters.Add(new SqliteParameter("@DateFrom", startDate));
                }
                if (dateTo.HasValue)
                {
                    DateTime endDate = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                    query += " AND Date <= @DateTo";
                    parameters.Add(new SqliteParameter("@DateTo", endDate));
                }
                if (categoryId.HasValue)
                {
                    query += " AND CategoryID = @CategoryID";
                    parameters.Add(new SqliteParameter("@CategoryID", categoryId.Value));
                }
                if (storeId.HasValue)
                {
                    query += " AND StoreID = @StoreID";
                    parameters.Add(new SqliteParameter("@StoreID", storeId.Value));
                }
                if (transactionTypeId.HasValue)
                {
                    query += " AND TransactionTypeID = @TransactionTypeID";
                    parameters.Add(new SqliteParameter("@TransactionTypeID", transactionTypeId.Value));
                }
                if (amountFrom.HasValue)
                {
                    query += " AND Amount >= @AmountFrom";
                    parameters.Add(new SqliteParameter("@AmountFrom", amountFrom.Value));
                }
                if (amountTo.HasValue)
                {
                    query += " AND Amount <= @AmountTo";
                    parameters.Add(new SqliteParameter("@AmountTo", amountTo.Value));
                }

                DataTable result = database.ExecuteQuery(query, parameters.ToArray());

                foreach (DataRow row in result.Rows)
                {
                    Transaction transaction = new Transaction
                    {
                        TransactionID = row["TransactionID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionID"]),
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        Amount = row["Amount"] == DBNull.Value ? 0.0m : Convert.ToDecimal(row["Amount"]),
                        TransactionTypeID = row["TransactionTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionTypeID"]),
                        CategoryID = row["CategoryID"] == DBNull.Value ? -1 : Convert.ToInt32(row["CategoryID"]),
                        SubcategoryID = row["SubcategoryID"] == DBNull.Value ? -1 : Convert.ToInt32(row["SubcategoryID"]),
                        StoreID = row["StoreID"] == DBNull.Value ? -1 : Convert.ToInt32(row["StoreID"]),
                        Note = row["Note"] == DBNull.Value ? null : Convert.ToString(row["Note"]),
                        Date = row["Date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["Date"])
                    };
                    transactions.Add(transaction);
                }
            }
            return transactions;
        }

        public static List<Transaction> GetFamilyTransactions(int familyId)
        {
            List<Transaction> familyTransactions = new List<Transaction>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;
                ObservableCollection<Transaction> userTransactions = GetUserTransactions(userId);

                foreach (Transaction transaction in userTransactions)
                {
                    if (!familyTransactions.Any(s => s.TransactionID == transaction.TransactionID))
                    {
                        familyTransactions.Add(transaction);
                    }
                }
            }
            return familyTransactions;
        }
        public static List<Transaction> GetFilteredFamilyTransactions(int familyId, int? filterUserId = null, DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null, int? storeId = null, int? transactionTypeId = null, double? amountFrom = null, double? amountTo = null)
        {
            List<Transaction> familyTransactions = new List<Transaction>();

            if (filterUserId.HasValue)
            {
                int userId = filterUserId.Value;
                List<Transaction> userTransactions = GetFilteredUserTransactions(userId: userId, dateFrom: startDate, dateTo: endDate, categoryId: categoryId, storeId: storeId, transactionTypeId: transactionTypeId, amountFrom: amountFrom, amountTo: amountTo);

                foreach (Transaction transaction in userTransactions)
                {
                    familyTransactions.Add(transaction);
                }
            }
            else
            {
                List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

                foreach (FamilyMember member in familyMembers)
                {
                    int userId = member.UserID;
                    List<Transaction> userTransactions = GetFilteredUserTransactions(userId: userId, dateFrom: startDate, dateTo: endDate, categoryId: categoryId, storeId: storeId, transactionTypeId: transactionTypeId, amountFrom: amountFrom, amountTo: amountTo);

                    foreach (Transaction transaction in userTransactions)
                    {
                        if (!familyTransactions.Any(s => s.TransactionID == transaction.TransactionID))
                        {
                            familyTransactions.Add(transaction);
                        }
                    }
                }
            }
            return familyTransactions;
        }

        public static ObservableCollection<Store> GetUserStores(int userId)
        {
            ObservableCollection<Store> stores = new ObservableCollection<Store>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = @"
            SELECT 
                Stores.StoreID, 
                Stores.StoreName, 
                Stores.UserID, 
                Stores.IsFavorite, 
                Categories.CategoryName
            FROM 
                Stores
            LEFT JOIN 
                Categories ON Stores.CategoryID = Categories.CategoryID
            WHERE 
                Stores.UserID = @UserID OR Stores.UserID IS NULL";

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@UserID", userId));

                foreach (DataRow row in result.Rows)
                {
                    Store store = new Store(
                        storeId: Convert.ToInt32(row["StoreID"]),
                        userId: row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"]),
                        isFavorite: Convert.ToBoolean(row["IsFavorite"])
                    )
                    {
                        StoreName = row["StoreName"].ToString(),
                        CategoryName = row["CategoryName"] == DBNull.Value ? null : row["CategoryName"].ToString()
                    };

                    stores.Add(store);
                }
            }
            return stores;
        }

        public static List<Store> GetFamilyStores(int familyId)
        {
            List<Store> familyStores = new List<Store>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;
                ObservableCollection<Store> userStores = GetUserStores(userId);

                foreach (Store store in userStores)
                {
                    if (!familyStores.Any(s => s.StoreId == store.StoreId))
                    {
                        familyStores.Add(store);
                    }
                }
            }
            return familyStores;
        }

        public static List<FamilyMember> GetFamilyMembersByFamilyId(int familyId)
        {
            List<FamilyMember> familyMembers = new List<FamilyMember>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT UserID, FamilyID, RoleID FROM Users WHERE FamilyID = @FamilyID";

                SqliteParameter familyIdParam = new SqliteParameter("@FamilyID", familyId);
                DataTable result = database.ExecuteQuery(query, familyIdParam);

                foreach (DataRow row in result.Rows)
                {
                    FamilyMember familyMember = new FamilyMember
                    {
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        FamilyID = row["FamilyID"] == DBNull.Value ? 0 : Convert.ToInt32(row["FamilyID"]),
                        RoleID = row["RoleID"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleID"])
                    };
                    familyMembers.Add(familyMember);
                }
            }
            return familyMembers;
        }

        public static List<TransactionType> GetTransactionTypes()
        {
            List<TransactionType> transactionTypes = new List<TransactionType>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT TransactionTypeID, TypeName FROM TransactionTypes";

                DataTable result = database.ExecuteQuery(query);

                foreach (DataRow row in result.Rows)
                {
                    TransactionType transactionType = new TransactionType
                    {
                        TransactionTypeID = row["TransactionTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionTypeID"]),
                        TypeName = row["TypeName"] == DBNull.Value ? null : Convert.ToString(row["TypeName"])
                    };
                    transactionTypes.Add(transactionType);
                }
            }
            return transactionTypes;
        }
    }
}
