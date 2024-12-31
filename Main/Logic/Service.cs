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
using Main.Controls;
using OxyPlot;
using System.Windows.Controls.Primitives;

namespace Main.Logic
{
    public static class Service
    {
        // Validation Methods
        public static bool IsEmailExistis(string email,int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                DataTable result;
                if (userId >0)
                {
                    string selectQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email AND UserID != @UserID";
                    result = database.ExecuteQuery(selectQuery, new SqliteParameter("@Email", email), new SqliteParameter("@UserID", userId));
                }
                else
                {
                    string selectQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
                    result = database.ExecuteQuery(selectQuery, new SqliteParameter("@Email", email));
                }
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

        public static bool IsStoreFavoriteForUser(int userId, int storeId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT COUNT(*) FROM FavoriteStores WHERE UserID = @UserID AND StoreID = @StoreID";
                var result = database.ExecuteQuery(selectQuery,
                        new SqliteParameter("@StoreID", storeId),
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

        public static bool AddTransaction(int userId, decimal amount, int transactionTypeId, int categoryId, int? subcategoryId, int? storeId, string note, DateTime date, out int? transactionId)
        {
            transactionId = null;
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = @"
                    INSERT INTO Transactions (UserID, Amount, TransactionTypeID, CategoryID, SubcategoryID, StoreID, Note, Date) 
                    VALUES (@UserID, @Amount, @TransactionTypeID, @CategoryID, @SubcategoryID, @StoreID, @Note, @Date)";

                try
                {
                    database.ExecuteNonQuery(
                        insertQuery,
                        new SqliteParameter("@UserID", userId),
                        new SqliteParameter("@Amount", amount),
                        new SqliteParameter("@TransactionTypeID", transactionTypeId),
                        new SqliteParameter("@CategoryID", categoryId),
                        new SqliteParameter("@SubcategoryID", subcategoryId.HasValue ? (object)subcategoryId.Value : DBNull.Value),
                        new SqliteParameter("@StoreID", storeId.HasValue ? (object)storeId.Value : DBNull.Value),
                        new SqliteParameter("@Note", note == null ? DBNull.Value : (object)note),
                        new SqliteParameter("@Date", date)
                    );

                    var idQuery = "SELECT LAST_INSERT_ROWID()";
                    var result = database.ExecuteQuery(idQuery);
                    if (result != null && result.Rows.Count > 0 && int.TryParse(result.Rows[0][0]?.ToString(), out var id))
                    {
                        transactionId = id;
                        return true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static bool AddRecurringPaymentHistory( int recurringPaymentId, int? transactionId, decimal amount, DateTime paymentDate, int actionTypeId, DateTime actionDate)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string insertQuery = @"
                    INSERT INTO RecurringPaymentHistory 
                    (RecurringPaymentID, TransactionID, Amount, PaymentDate, ActionTypeID, ActionDate) 
                    VALUES (@RecurringPaymentID, @TransactionID, @Amount, @PaymentDate, @ActionTypeID, @ActionDate)";

                try
                {
                    database.ExecuteNonQuery(
                        insertQuery,
                        new SqliteParameter("@RecurringPaymentID", recurringPaymentId),
                        new SqliteParameter("@TransactionID", transactionId.HasValue ? (object)transactionId.Value : DBNull.Value),
                        new SqliteParameter("@Amount", amount),
                        new SqliteParameter("@PaymentDate", paymentDate),
                        new SqliteParameter("@ActionTypeID", actionTypeId),
                        new SqliteParameter("@ActionDate", actionDate)
                    );

                    return true; 
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static bool AddTransactionsFromRecurringPayments(List<RecurringPayment> recurringPayments)
        {
            if (recurringPayments!=null && recurringPayments.Any())
            {
                foreach (var payment in recurringPayments)
                {
                    var lastPaymentDate = GetLastPaymentDateForRecurringPayment(payment.RecurringPaymentID);
                    var startDate = (lastPaymentDate == null || payment.PaymentDate > lastPaymentDate)
                        ? payment.PaymentDate
                        : lastPaymentDate.Value;
                    var nextPaymentDate = GetNextPaymentDate(payment.FrequencyID, startDate);

                    if (nextPaymentDate == null)
                    {
                        Console.WriteLine("Brak odpowiedniej daty płatności do dalszego przetwarzania.");
                        return false;
                    }

                    while (nextPaymentDate <= DateTime.Now)
                    {
                        var successAddTransaction = Service.AddTransaction(payment.UserID, payment.Amount, payment.TransactionTypeID ?? -1, payment.CategoryID ?? -1, null, payment.StoreID, $"Cykliczna płatność: {payment.RecurringPaymentName}", nextPaymentDate ?? DateTime.Now, out int? transactionId);
                        if (successAddTransaction && transactionId!=null)
                        {
                            var successAddRecurringHistory = Service.AddRecurringPaymentHistory(
                                payment.RecurringPaymentID,
                                transactionId,
                                payment.Amount,
                                nextPaymentDate ?? DateTime.Now,
                                1,
                                nextPaymentDate ?? DateTime.Now
                            );
                            if (successAddRecurringHistory)
                            {
                                nextPaymentDate = GetNextPaymentDate(payment.FrequencyID, nextPaymentDate ?? DateTime.Now);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
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
        public static bool UpdateUserEmailAndUsername(int userId, string newEmail, string newUsername)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateQuery = @"
                    UPDATE Users 
                    SET Email = @Email, UserName = @UserName 
                    WHERE UserID = @UserID";

                var parameters = new[]
                {
                    new SqliteParameter("@Email", newEmail),
                    new SqliteParameter("@UserName", newUsername),
                    new SqliteParameter("@UserID", userId)
                };

                int rowsAffected = database.ExecuteNonQuery(updateQuery, parameters);

                return rowsAffected > 0;
            }
        }

        public static bool UpdateUserPassword(int userId, string newPassword)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string newSalt = GenerateSalt();
                string hashNewPassword = HashPassword(Encoding.UTF8.GetBytes(newPassword), Encoding.UTF8.GetBytes(newSalt));

                string updateQuery = @"
                    UPDATE Users 
                    SET PasswordHash = @PasswordHash, Salt = @Salt 
                    WHERE UserID = @UserID";

                var parameters = new[]
                {
                    new SqliteParameter("@PasswordHash", hashNewPassword),
                    new SqliteParameter("@Salt", newSalt),
                    new SqliteParameter("@UserID", userId)
                };

                int rowsAffected = database.ExecuteNonQuery(updateQuery, parameters);

                return rowsAffected > 0;
            }
        }
        public static bool ActivateRecurringPayment(int recurringPaymentID)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string updateQuery = "UPDATE RecurringPayments SET PaymentDate = @PaymentDate, IsActive = 1 WHERE RecurringPaymentID = @RecurringPaymentID";
                try
                {
                    database.ExecuteNonQuery(updateQuery,
                        new SqliteParameter("@PaymentDate", DateTime.Today),
                        new SqliteParameter("@RecurringPaymentID", recurringPaymentID));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool UpdateNotesForRecurringTransactions(int recurringPaymentId, string newPaymentName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try {
                    if (string.IsNullOrWhiteSpace(newPaymentName))
                    {
                        return false;
                    }
                    string noteContent = $"Cykliczna płatność: {newPaymentName}";

                    string selectQuery = @" SELECT TransactionID FROM RecurringPaymentHistory WHERE RecurringPaymentID = @RecurringPaymentID AND TransactionID IS NOT NULL";
                    List<int> transactionIds = new List<int>();

                    DataTable result = database.ExecuteQuery(
                        selectQuery,
                        new SqliteParameter("@RecurringPaymentID", recurringPaymentId)
                        );

                    if (result != null && result.Rows.Count != 0) { 
                        foreach (DataRow row in result.Rows)
                        {
                            if (row["TransactionID"] != DBNull.Value)
                            {
                                transactionIds.Add(Convert.ToInt32(row["TransactionID"]));
                            }
                        }
                    }

                    string updateQuery = @"UPDATE Transactions SET Note = @Note WHERE TransactionID = @TransactionID"; 
                    if (transactionIds != null && transactionIds.Count != 0)
                    {
                        foreach (int transactionId in transactionIds)
                        {
                            try
                            {
                                database.ExecuteNonQuery(updateQuery,
                                    new SqliteParameter("@Note", noteContent),
                                    new SqliteParameter("@TransactionID", transactionId));
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        }
                    }

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
                    string deleteSubcategoriesQuery = "DELETE FROM Subcategories WHERE CategoryID = @CategoryId";
                    database.ExecuteNonQuery(deleteSubcategoriesQuery, new SqliteParameter("@CategoryId", categoryId));

                    string deleteFavoriteCategoriesQuery = "DELETE FROM FavoriteCategories WHERE CategoryID = @CategoryId";
                    database.ExecuteNonQuery(deleteFavoriteCategoriesQuery, new SqliteParameter("@CategoryId", categoryId));

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

        public static bool DeleteUser(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string deleteFavoriteCategoriesQuery = "DELETE FROM FavoriteCategories WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteFavoriteCategoriesQuery, new SqliteParameter("@UserID", userId));

                    string deleteFavouriteStoresQuery = "DELETE FROM FavoriteStores WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteFavouriteStoresQuery, new SqliteParameter("@UserID", userId));

                    string deleteNotificationsQuery = "DELETE FROM Notifications WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteNotificationsQuery, new SqliteParameter("@UserID", userId));

                    string deleteGoalsQuery = "DELETE FROM Goals WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteGoalsQuery, new SqliteParameter("@UserID", userId));

                    string deleteLimitsQuery = "DELETE FROM Limits WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteLimitsQuery, new SqliteParameter("@UserID", userId));

                    string deleteReccuringPaymentHistoryQuery = "DELETE FROM RecurringPaymentHistory WHERE RecurringPaymentID IN (SELECT RecurringPaymentID FROM RecurringPayments WHERE UserID = @UserID);";
                    database.ExecuteNonQuery(deleteReccuringPaymentHistoryQuery, new SqliteParameter("@UserID", userId));

                    string deleteRecurringPaymentsQuery = "DELETE FROM RecurringPayments WHERE UserID = @UserID OR CreatedByUserID = @UserID;";
                    database.ExecuteNonQuery(deleteRecurringPaymentsQuery, new SqliteParameter("@UserID", userId));

                    string deleteTransactionsQuery = "DELETE FROM Transactions WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteTransactionsQuery, new SqliteParameter("@UserID", userId));

                    string deleteJoinRequestssQuery = "DELETE FROM JoinRequests WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteJoinRequestssQuery, new SqliteParameter("@UserID", userId));

                    string deleteStoresQuery = "DELETE FROM Stores WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteStoresQuery, new SqliteParameter("@UserID", userId));

                    string deleteSubcategoriesQuery = "DELETE FROM Subcategories WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteSubcategoriesQuery, new SqliteParameter("@UserID", userId));

                    string deleteCategoriesQuery = "DELETE FROM Categories WHERE UserID = @UserID;";
                    database.ExecuteNonQuery(deleteCategoriesQuery, new SqliteParameter("@UserID", userId));

                    string updateUsersQuery = "UPDATE Users SET FamilyID = NULL WHERE FamilyID = (SELECT FROM Family WHERE PrimaryUserID = @UserID);";
                    database.ExecuteNonQuery(updateUsersQuery, new SqliteParameter("@UserID", userId));

                    string deleteFamilyGoalsQuery = "DELETE FROM Goals WHERE FamilyID = @FamilyID;\r\n";
                    database.ExecuteNonQuery(deleteFamilyGoalsQuery, new SqliteParameter("@UserID", userId));

                    string deleteFamilyLimitsQuery = "DELETE FROM Limits WHERE FamilyID = @FamilyID;\r\n";
                    database.ExecuteNonQuery(deleteFamilyLimitsQuery, new SqliteParameter("@UserID", userId));

                    string deleteFamilyJoinRequestsQuery = "DELETE FROM JoinRequests WHERE FamilyID = @FamilyID;\r\n";
                    database.ExecuteNonQuery(deleteFamilyJoinRequestsQuery, new SqliteParameter("@UserID", userId));

                    string deleteFamilyQuery = "DELETE FROM Family WHERE PrimaryUserID = @UserID;";
                    database.ExecuteNonQuery(deleteFamilyQuery, new SqliteParameter("@UserID", userId));

                    string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserID;\r\n";
                    database.ExecuteNonQuery(deleteUserQuery, new SqliteParameter("@UserID", userId));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public static bool DeleteStore(int storeId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string deleteFavoriteCategoriesQuery = "DELETE FROM FavoriteStores WHERE StoreID = @StoreID";
                    database.ExecuteNonQuery(deleteFavoriteCategoriesQuery, new SqliteParameter("@StoreID", storeId));

                    string deleteCategoriesQuery = "DELETE FROM Stores WHERE StoreID = @StoreID";
                    database.ExecuteNonQuery(deleteCategoriesQuery, new SqliteParameter("@StoreID", storeId));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // GET Methods

        public static User GetUserByUserID(int userId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT * FROM Users WHERE UserID = @UserID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@UserID", userId));

                if (result == null || result.Rows.Count == 0)
                {
                    return null; 
                }

                var row = result.Rows[0];

                return new User
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    UserName = row["UserName"].ToString(),
                    Email = row["Email"].ToString(),
                    PasswordHash = row["PasswordHash"].ToString(),
                    Salt = row["Salt"].ToString(),
                    RoleID = Convert.ToInt32(row["RoleID"]),
                    FamilyID = row["FamilyID"] != DBNull.Value ? (int?)Convert.ToInt32(row["FamilyID"]) : null,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    ProfileSettings = row["ProfileSettings"]?.ToString()
                };
            }
        }

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

        public static int GetUserIdByRecurringPaymentId(int recurringPaymentId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT UserID FROM RecurringPayments WHERE RecurringPaymentID = @RecurringPaymentID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@RecurringPaymentID", recurringPaymentId));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                return result.Rows[0]["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(result.Rows[0]["UserID"]);
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

        public static string GetRoleNameByRoleID(int roleId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT RoleName FROM Roles WHERE RoleID = @RoleID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@RoleID", roleId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["RoleName"].ToString();
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

        public static List<JoinRequest> GetPendingJoinRequestsByFamilyId(int familyId)
        {
            List<JoinRequest> joinRequests = new List<JoinRequest>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT JoinRequests.JoinRequestID, Family.FamilyName, " +
                       "JoinRequests.JoinRequestDate, RequestStatuses.RequestStatusName, JoinRequests.UserID " +
                       "FROM JoinRequests " +
                       "INNER JOIN Family ON JoinRequests.FamilyID = Family.FamilyID " +
                       "INNER JOIN RequestStatuses ON JoinRequests.RequestStatusID = RequestStatuses.RequestStatusID " +
                       "WHERE JoinRequests.FamilyID = @FamilyID " +
                       "AND JoinRequests.RequestStatusID = 1"; 

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@FamilyID", familyId));

                foreach (DataRow row in result.Rows)
                {
                    JoinRequest request = new JoinRequest
                    {
                        JoinRequestID = Convert.ToInt32(row["JoinRequestID"]),
                        FamilyName = row["FamilyName"].ToString(),
                        UserID = Convert.ToInt32(row["UserID"]),
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

        public static int GetCategoryIDByCategoryName(string categoryName)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT CategoryID FROM Categories WHERE CategoryName = @CategoryName";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@CategoryName", categoryName));
                if (result == null || result.Rows.Count == 0)
                {
                    return -1;
                }
                return Convert.ToInt32(result.Rows[0]["CategoryID"]);
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

                if (userId.HasValue && userId.Value != -1)
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
                if (categoryId.HasValue && categoryId.Value!=-1)
                {
                    query += " AND CategoryID = @CategoryID";
                    parameters.Add(new SqliteParameter("@CategoryID", categoryId.Value));
                }
                if (storeId.HasValue && storeId.Value != -1)
                {
                    query += " AND StoreID = @StoreID";
                    parameters.Add(new SqliteParameter("@StoreID", storeId.Value));
                }
                if (transactionTypeId.HasValue && transactionTypeId.Value != -1)
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

            if (filterUserId.HasValue && filterUserId.Value!=-1)
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

        public static ObservableCollection<Store> GetUserStoresByCategory(int userId, int categoryId)
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
                        (Stores.UserID = @UserID OR Stores.UserID IS NULL) 
                        AND Stores.CategoryID = @CategoryID";

                DataTable result = database.ExecuteQuery(
                    query,
                    new SqliteParameter("@UserID", userId),
                    new SqliteParameter("@CategoryID", categoryId)
                );

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

        public static ObservableCollection<Store> GetFamilyStores(int familyId)
        {
            ObservableCollection<Store> familyStores = new ObservableCollection<Store>();
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

        public static ObservableCollection<Store> GetFamilyStoresByCategory(int familyId, int categoryId)
        {
            ObservableCollection<Store> familyStores = new ObservableCollection<Store>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;
                ObservableCollection<Store> userStores = GetUserStoresByCategory(userId, categoryId);

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

        public static string GetRecurringPaymentNameByRecurringPaymentID(int recurringPaymentId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT RecurringPaymentName FROM RecurringPayments WHERE RecurringPaymentID = @RecurringPaymentID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@RecurringPaymentID", recurringPaymentId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["RecurringPaymentName"].ToString();
            }
        }


        public static List<RecurringPaymentHistory> GetHistoryByRecurringPaymentID(int recurringPaymentID)
        {
            List<RecurringPaymentHistory> historyList = new List<RecurringPaymentHistory>();

            using (DBSqlite database = new DBSqlite())
            {
                try
                {
                    string query = @"
                        SELECT RecurringPaymentHistoryID, RecurringPaymentID, TransactionID, Amount, PaymentDate, ActionTypeID, ActionDate 
                        FROM RecurringPaymentHistory 
                        WHERE RecurringPaymentID = @RecurringPaymentID";

                    SqliteParameter recurringPaymentIdParam = new SqliteParameter("@RecurringPaymentID", recurringPaymentID);
                    DataTable result = database.ExecuteQuery(query, recurringPaymentIdParam);

                    foreach (DataRow row in result.Rows)
                    {
                        var history = new RecurringPaymentHistory
                        {
                            RecurringPaymentHistoryID = row["RecurringPaymentHistoryID"] != DBNull.Value ? Convert.ToInt32(row["RecurringPaymentHistoryID"]) : 0,
                            RecurringPaymentID = row["RecurringPaymentID"] != DBNull.Value ? Convert.ToInt32(row["RecurringPaymentID"]) : 0,
                            TransactionID = row["TransactionID"] != DBNull.Value ? Convert.ToInt32(row["TransactionID"]) : (int?)null,
                            Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0m,
                            PaymentDate = row["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(row["PaymentDate"]) : DateTime.MinValue,
                            ActionTypeID = row["ActionTypeID"] != DBNull.Value ? Convert.ToInt32(row["ActionTypeID"]) : 0,
                            ActionDate = row["ActionDate"] != DBNull.Value ? Convert.ToDateTime(row["ActionDate"]) : DateTime.MinValue
                        };

                        historyList.Add(history);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas pobierania historii płatności: {ex.Message}");
                }
            }
            return historyList; 
        }

        public static List<RecurringPayment> GetRecurringPaymentsByUserId(int userId)
        {
            List<RecurringPayment> recurringPayments = new List<RecurringPayment>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = @"
                SELECT RecurringPaymentID, RecurringPaymentName, UserID, StoreID, CategoryID, Amount, TransactionTypeID, PaymentDate, FrequencyID, IsActive, CreatedByUserID
                FROM RecurringPayments
                WHERE UserID = @UserId";

                SqliteParameter userIdParam = new SqliteParameter("@UserId", userId);
                DataTable result = database.ExecuteQuery(query, userIdParam);

                foreach (DataRow row in result.Rows)
                {
                    recurringPayments.Add(new RecurringPayment
                    {
                        RecurringPaymentID = Convert.ToInt32(row["RecurringPaymentID"]),
                        RecurringPaymentName = Convert.ToString(row["RecurringPaymentName"]),
                        UserID = Convert.ToInt32(row["UserID"]),
                        StoreID = row["StoreID"] != DBNull.Value ? Convert.ToInt32(row["StoreID"]) : (int?)null,
                        CategoryID = row["CategoryID"] != DBNull.Value ? Convert.ToInt32(row["CategoryID"]) : (int?)null,
                        TransactionTypeID = row["TransactionTypeID"] != DBNull.Value ? Convert.ToInt32(row["TransactionTypeID"]) : (int?)null,
                        Amount = Convert.ToDecimal(row["Amount"]),
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        FrequencyID = Convert.ToInt32(row["FrequencyID"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedByUserID = Convert.ToInt32(row["CreatedByUserID"]),
                    });
                }
            }

            return recurringPayments;
        }

        public static List<RecurringPayment> GetActiveRecurringPaymentsByUserId(int userId)
        {
            List<RecurringPayment> recurringPayments = new List<RecurringPayment>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = @"
                SELECT RecurringPaymentID, RecurringPaymentName, UserID, StoreID, CategoryID, Amount, TransactionTypeID, PaymentDate, FrequencyID, IsActive, CreatedByUserID
                FROM RecurringPayments
                WHERE UserID = @UserId AND IsActive = 1";

                SqliteParameter userIdParam = new SqliteParameter("@UserId", userId);
                DataTable result = database.ExecuteQuery(query, userIdParam);

                foreach (DataRow row in result.Rows)
                {
                    recurringPayments.Add(new RecurringPayment
                    {
                        RecurringPaymentID = Convert.ToInt32(row["RecurringPaymentID"]),
                        RecurringPaymentName = Convert.ToString(row["RecurringPaymentName"]),
                        UserID = Convert.ToInt32(row["UserID"]),
                        StoreID = row["StoreID"] != DBNull.Value ? Convert.ToInt32(row["StoreID"]) : (int?)null,
                        CategoryID = row["CategoryID"] != DBNull.Value ? Convert.ToInt32(row["CategoryID"]) : (int?)null,
                        TransactionTypeID = row["TransactionTypeID"] != DBNull.Value ? Convert.ToInt32(row["TransactionTypeID"]) : (int?)null,
                        Amount = Convert.ToDecimal(row["Amount"]),
                        PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                        FrequencyID = Convert.ToInt32(row["FrequencyID"]),
                        IsActive = Convert.ToBoolean(row["IsActive"]),
                        CreatedByUserID = Convert.ToInt32(row["CreatedByUserID"]),
                    });
                }
            }

            return recurringPayments;
        }

        public static List<RecurringPayment> GetFamilyRecurringPayments(int familyId)
        {
            List<RecurringPayment> familyRecurringPayments = new List<RecurringPayment>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;

                List<RecurringPayment> userRecurringPayments = GetRecurringPaymentsByUserId(userId);

                foreach (RecurringPayment recurringPayment in userRecurringPayments)
                {
                    if (!familyRecurringPayments.Any(t => t.RecurringPaymentID == recurringPayment.RecurringPaymentID))
                    {
                        familyRecurringPayments.Add(recurringPayment);
                    }
                }
            }

            return familyRecurringPayments;
        }

        public static List<RecurringPayment> GetActiveRecurringPaymentsByFamilyId(int familyId)
        {
            List<RecurringPayment> familyRecurringPayments = new List<RecurringPayment>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;

                List<RecurringPayment> userRecurringPayments = GetActiveRecurringPaymentsByUserId(userId);

                foreach (RecurringPayment recurringPayment in userRecurringPayments)
                {
                    if (!familyRecurringPayments.Any(t => t.RecurringPaymentID == recurringPayment.RecurringPaymentID))
                    {
                        familyRecurringPayments.Add(recurringPayment);
                    }
                }
            }

            return familyRecurringPayments;
        }

        private static DateTime? GetNextPaymentDate(int frequencyId, DateTime lastPaymentDate)
        {
            DateTime baseDate = lastPaymentDate;

            if (frequencyId == 1)
            {
                return baseDate.AddDays(1);       // Codziennie
            }
            else if (frequencyId == 2)
            {
                return baseDate.AddDays(7);       // Co tydzień
            }
            else if (frequencyId == 3)
            {
                return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day).AddMonths(1);     // Co miesiąc
            }
            else if (frequencyId == 4)
            {
                return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day).AddMonths(3);     // Co kwartał
            }
            else if (frequencyId == 5)
            {
                return new DateTime(baseDate.Year + 1, baseDate.Month, baseDate.Day); ;      // Co rok
            }

            return null;
        }

        public static DateTime? GetLastPaymentDateForRecurringPayment(int recurringPaymentId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                var query = "SELECT MAX(PaymentDate) AS LastPaymentDate FROM RecurringPaymentHistory WHERE RecurringPaymentID = @RecurringPaymentID";
                var result = database.ExecuteQuery(query, new SqliteParameter("@RecurringPaymentID", recurringPaymentId));
                if (result == null || result.Rows.Count == 0 || result.Rows[0]["LastPaymentDate"] == DBNull.Value)
                {
                    return null;
                }
                if (DateTime.TryParse(result.Rows[0]["LastPaymentDate"].ToString(), out DateTime lastPaymentDate))
                {
                    return lastPaymentDate;
                }

                return null;
            }
        }

        public static string GetActionNameByActionID(int actionTypeId)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT TypeName FROM ActionTypes WHERE ActionTypeID = @ActionTypeID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@ActionTypeID", actionTypeId));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["TypeName"].ToString();
            }
        }

        public static string GetFrequencyNameByFrequencyID(int frequencyID)
        {
            using (DBSqlite database = new DBSqlite())
            {
                string selectQuery = "SELECT FrequencyName FROM Frequencies WHERE FrequencyID = @FrequencyID";
                var result = database.ExecuteQuery(selectQuery, new SqliteParameter("@FrequencyID", frequencyID));
                if (result == null || result.Rows.Count == 0)
                {
                    return null;
                }
                return result.Rows[0]["FrequencyName"].ToString();
            }
        }

        public static ObservableCollection<Limit> GetUserLimits(int userId)
        {
            ObservableCollection<Limit> limits = new ObservableCollection<Limit>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = @"SELECT LimitID, FamilyID, UserID, CategoryID, LimitAmount, FrequencyID, IsFamilyWide, CreatedByUserID FROM Limits WHERE UserID = @UserID";

                DataTable result = database.ExecuteQuery(query, new SqliteParameter("@UserID", userId));

                foreach (DataRow row in result.Rows)
                {
                    Limit limit = new Limit(
                        limitId: Convert.ToInt32(row["LimitID"]),
                        familyId: row["FamilyID"] == DBNull.Value ? -1 : Convert.ToInt32(row["FamilyID"]),
                        userId: row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"]),
                        categoryId: Convert.ToInt32(row["CategoryID"]),
                        limitAmount: Convert.ToDouble(row["LimitAmount"]),
                        frequencyId: Convert.ToInt32(row["FrequencyID"]),
                        isFamilyWide: Convert.ToInt32(row["IsFamilyWide"]),
                        createdByUserID: Convert.ToInt32(row["CreatedByUserID"])
                    );

                    limits.Add(limit);
                }
            }
            return limits;
        }

        public static ObservableCollection<Limit> GetFilteredUserLimits(int? isFamilyWide = null, int? userId = null, int? categoryId = null, int? frequencyId = null, double? minAmount = null, double? maxAmount = null, bool? isExceededYes = null, bool? isExceededNo = null)
        {
            ObservableCollection<Limit> limits = new ObservableCollection<Limit>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = @"SELECT LimitID, FamilyID, UserID, CategoryID, LimitAmount, FrequencyID, IsFamilyWide, CreatedByUserID
                         FROM Limits
                         WHERE 1=1";
                List<SqliteParameter> parameters = new List<SqliteParameter>();
                if (userId.HasValue && userId.Value !=1)
                {
                    query += " AND UserID = @UserID";
                    parameters.Add(new SqliteParameter("@UserID", userId.Value));
                }

                if (isFamilyWide.HasValue && isFamilyWide.Value-1 != -1)
                {
                    query += " AND IsFamilyWide = @IsFamilyWide";
                    parameters.Add(new SqliteParameter("@IsFamilyWide", isFamilyWide.Value-1));
                }
                if (categoryId.HasValue && categoryId.Value != -1)
                {
                    query += " AND CategoryID = @CategoryID";
                    parameters.Add(new SqliteParameter("@CategoryID", categoryId.Value));
                }
                if (frequencyId.HasValue && frequencyId.Value != -1)
                {
                    query += " AND FrequencyId = @FrequencyId";
                    parameters.Add(new SqliteParameter("@FrequencyId", frequencyId.Value));
                }
                if (minAmount.HasValue)
                {
                    query += " AND LimitAmount >= @MinAmount";
                    parameters.Add(new SqliteParameter("@MinAmount", minAmount.Value));
                }
                if (maxAmount.HasValue)
                {
                    query += " AND LimitAmount <= @MaxAmount";
                    parameters.Add(new SqliteParameter("@MaxAmount", maxAmount.Value));
                }

                DataTable result = database.ExecuteQuery(query, parameters.ToArray());

                foreach (DataRow row in result.Rows)
                {
                    Limit limit = new Limit(
                        limitId: Convert.ToInt32(row["LimitID"]),
                        familyId: row["FamilyID"] == DBNull.Value ? -1 : Convert.ToInt32(row["FamilyID"]),
                        userId: row["UserID"] == DBNull.Value ? -1 : Convert.ToInt32(row["UserID"]),
                        categoryId: Convert.ToInt32(row["CategoryID"]),
                        limitAmount: Convert.ToDouble(row["LimitAmount"]),
                        frequencyId: Convert.ToInt32(row["FrequencyID"]),
                        isFamilyWide: Convert.ToInt32(row["IsFamilyWide"]),
                        createdByUserID: Convert.ToInt32(row["CreatedByUserID"])
                    );

                    limits.Add(limit);
                }
            }
            return limits;
        }

        public static ObservableCollection<Limit> GetFamilyLimits(int familyId)
        {
            ObservableCollection<Limit> familyLimits = new ObservableCollection<Limit>();
            List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

            foreach (FamilyMember member in familyMembers)
            {
                int userId = member.UserID;
                ObservableCollection<Limit> userLimits = GetUserLimits(userId);

                foreach (Limit limit in userLimits)
                {
                    if (!familyLimits.Any(s => s.LimitId == limit.LimitId))
                    {
                        familyLimits.Add(limit);
                    }
                }
            }
            return familyLimits;
        }

        public static ObservableCollection<Limit> GetFilteredFamilyLimits(int familyId, int? isFamilyWide = null, int? filterUserId = null, int? categoryId = null, int? frequencyId = null, double? minAmount = null, double? maxAmount = null, bool? isExceededYes = null, bool? isExceededNo = null)
        {
            ObservableCollection<Limit> familyLimits = new ObservableCollection<Limit>();

            if (filterUserId.HasValue && filterUserId.Value != -1)
            {
                int userId = filterUserId.Value;
                ObservableCollection<Limit> userLimits = GetFilteredUserLimits(isFamilyWide, userId, categoryId, frequencyId, minAmount, maxAmount, isExceededYes, isExceededNo);

                foreach (Limit limit in userLimits)
                {
                    if (!familyLimits.Any(s => s.LimitId == limit.LimitId))
                    {
                        familyLimits.Add(limit);
                    }
                }
            }
            else
            {
                List<FamilyMember> familyMembers = GetFamilyMembersByFamilyId(familyId);

                foreach (FamilyMember member in familyMembers)
                {
                    int userId = member.UserID;
                    ObservableCollection<Limit> userLimits = GetFilteredUserLimits(isFamilyWide, userId, categoryId, frequencyId, minAmount, maxAmount, isExceededYes, isExceededNo);

                    foreach (Limit limit in userLimits)
                    {
                        if (!familyLimits.Any(s => s.LimitId == limit.LimitId))
                        {
                            familyLimits.Add(limit);
                        }
                    }
                }
            }
            return familyLimits;
        }

        public static List<User> GetChildrenByFamilyId(int familyId)
        {
            List<User> children = new List<User>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT UserID, UserName, Email,PasswordHash,Salt,RoleID,FamilyID,CreatedAt,ProfileSettings FROM Users WHERE FamilyID = @FamilyID AND RoleID = 3";
                

                SqliteParameter familyIdParam = new SqliteParameter("@FamilyID", familyId);
                DataTable result = database.ExecuteQuery(query, familyIdParam);

                foreach (DataRow row in result.Rows)
                {
                    User child = new User
                    {
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        UserName = row["UserName"] == DBNull.Value ? string.Empty : row["UserName"].ToString(),
                        Email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString(),
                        PasswordHash = row["PasswordHash"] == DBNull.Value ? string.Empty : row["PasswordHash"].ToString(),
                        Salt = row["Salt"] == DBNull.Value ? string.Empty : row["Salt"].ToString(),
                        RoleID = row["RoleID"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleID"]),
                        FamilyID = row["FamilyID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["FamilyID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        ProfileSettings = row["ProfileSettings"] == DBNull.Value ? string.Empty : row["ProfileSettings"].ToString()
                    };
                    children.Add(child);
                }
            }
            return children;
        }

        public static List<User> GetUsersByFamilyId(int familyId)
        {
            List<User> users = new List<User>();
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT UserID, UserName, Email,PasswordHash,Salt,RoleID,FamilyID,CreatedAt,ProfileSettings FROM Users WHERE FamilyID = @FamilyID";


                SqliteParameter familyIdParam = new SqliteParameter("@FamilyID", familyId);
                DataTable result = database.ExecuteQuery(query, familyIdParam);

                foreach (DataRow row in result.Rows)
                {
                    User user = new User
                    {
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        UserName = row["UserName"] == DBNull.Value ? string.Empty : row["UserName"].ToString(),
                        Email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString(),
                        PasswordHash = row["PasswordHash"] == DBNull.Value ? string.Empty : row["PasswordHash"].ToString(),
                        Salt = row["Salt"] == DBNull.Value ? string.Empty : row["Salt"].ToString(),
                        RoleID = row["RoleID"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleID"]),
                        FamilyID = row["FamilyID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["FamilyID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        ProfileSettings = row["ProfileSettings"] == DBNull.Value ? string.Empty : row["ProfileSettings"].ToString()
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        public static User GetUserByUserId(int userId)
        {
            User user = null;
            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT UserID, UserName, Email, PasswordHash, Salt, RoleID, FamilyID, ProfileSettings,CreatedAt FROM Users WHERE UserID = @UserID";

                SqliteParameter userIdParam = new SqliteParameter("@UserID", userId);
                DataTable result = database.ExecuteQuery(query, userIdParam);

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    user = new User
                    {
                        UserID = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                        UserName = row["UserName"] == DBNull.Value ? string.Empty : row["UserName"].ToString(),
                        Email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString(),
                        PasswordHash = row["PasswordHash"] == DBNull.Value ? string.Empty : row["PasswordHash"].ToString(),
                        Salt = row["Salt"] == DBNull.Value ? string.Empty : row["Salt"].ToString(),
                        RoleID = row["RoleID"] == DBNull.Value ? 0 : Convert.ToInt32(row["RoleID"]),
                        FamilyID = row["FamilyID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["FamilyID"]),
                        CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                        ProfileSettings = row["ProfileSettings"] == DBNull.Value ? string.Empty : row["ProfileSettings"].ToString()
                    };
                }
            }
            return user;
        }

        public static List<Frequency> GetFrequencies()
        {
            List<Frequency> frequencies = new List<Frequency>();

            using (DBSqlite database = new DBSqlite())
            {
                string query = "SELECT FrequencyID, FrequencyName FROM Frequencies";

                DataTable result = database.ExecuteQuery(query);

                foreach (DataRow row in result.Rows)
                {
                    Frequency frequency = new Frequency
                    {
                        FrequencyID = row["FrequencyID"] == DBNull.Value ? 0 : Convert.ToInt32(row["FrequencyID"]),
                        FrequencyName = row["FrequencyName"] == DBNull.Value ? null : Convert.ToString(row["FrequencyName"])
                    };
                    frequencies.Add(frequency);
                }
            }
            return frequencies;
        }

    }
}
