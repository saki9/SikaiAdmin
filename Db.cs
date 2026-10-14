using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SikaiAdmin
{
    public enum Role { Student, Teacher }

    public class AdminAccount
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string PasswordEntry { get; set; }
    }

    public class UserRow
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class ClassRow
    {
        public long Id { get; set; }
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public string TeacherName { get; set; }
        public int StudentCount { get; set; }
    }

    public static class Db
    {
        
        private static string Table(Role role) { return role == Role.Teacher ? "Teacher" : "Student"; }
        private static string IdColumn(Role role) { return role == Role.Teacher ? "TeacherID" : "StudentID"; }
        private static string Sequence(Role role) { return role == Role.Teacher ? "teacher_seq" : "student_seq"; }

        public static Task<OracleConnection> OpenAsync()
        {
            
            
            
            
            return Task.Run(() =>
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["OracleDb"];
                if (setting == null)
                    throw new ConfigurationErrorsException("Connection string 'OracleDb' was not found in App.config.");

                var conn = new OracleConnection(setting.ConnectionString);
                conn.Open();
                return conn;
            });
        }

        

        public static async Task<AdminAccount> FindAdminAsync(string email)
        {
            const string sql = "SELECT AdminID, FullName, Password FROM AdminAccount WHERE LOWER(Email) = :email";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("email", email.Trim().ToLowerInvariant()));

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;

                    return new AdminAccount
                    {
                        Id = Convert.ToInt64(reader.GetValue(0)),
                        FullName = reader.GetString(1),
                        PasswordEntry = reader.GetString(2)
                    };
                }
            }
        }

        
        
        public static async Task<AdminAccount> FindFirstAdminAsync()
        {
            const string sql =
                "SELECT AdminID, FullName, Password FROM AdminAccount " +
                "ORDER BY AdminID FETCH FIRST 1 ROWS ONLY";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    return null;

                return new AdminAccount
                {
                    Id = Convert.ToInt64(reader.GetValue(0)),
                    FullName = reader.GetString(1),
                    PasswordEntry = reader.GetString(2)
                };
            }
        }

        

        public static async Task<List<UserRow>> ListUsersAsync(Role role)
        {
            string sql = "SELECT " + IdColumn(role) + ", FullName, Email FROM " + Table(role) + " ORDER BY FullName";
            var list = new List<UserRow>();

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new UserRow
                    {
                        Id = Convert.ToInt64(reader.GetValue(0)),
                        FullName = reader.GetString(1),
                        Email = reader.GetString(2)
                    });
                }
            }
            return list;
        }

        public static async Task CreateUserAsync(Role role, string fullName, string email,
            string passwordEntry, string question, string answerEntry)
        {
            string sql =
                "INSERT INTO " + Table(role) + " (" + IdColumn(role) + ", FullName, Email, Password, SecurityQuestion, SecurityAnswer) " +
                "VALUES (" + Sequence(role) + ".NEXTVAL, :fname, :email, :pw, :q, :a)";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("fname", fullName));
                cmd.Parameters.Add(new OracleParameter("email", email));
                cmd.Parameters.Add(new OracleParameter("pw", passwordEntry));
                cmd.Parameters.Add(new OracleParameter("q", question));
                cmd.Parameters.Add(new OracleParameter("a", answerEntry));
                await cmd.ExecuteNonQueryAsync(); 
            }
        }

        
        
        public static async Task DeleteUserAsync(Role role, long id)
        {
            string sql = "DELETE FROM " + Table(role) + " WHERE " + IdColumn(role) + " = :id";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("id", id));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task UpdatePasswordAsync(Role role, long id, string passwordEntry)
        {
            string sql = "UPDATE " + Table(role) + " SET Password = :pw WHERE " + IdColumn(role) + " = :id";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("pw", passwordEntry));
                cmd.Parameters.Add(new OracleParameter("id", id));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<UserRow> FindUserAsync(Role role, string email)
        {
            string sql = "SELECT " + IdColumn(role) + ", FullName, Email FROM " + Table(role) +
                         " WHERE LOWER(Email) = :email";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("email", email.Trim().ToLowerInvariant()));

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;

                    return new UserRow
                    {
                        Id = Convert.ToInt64(reader.GetValue(0)),
                        FullName = reader.GetString(1),
                        Email = reader.GetString(2)
                    };
                }
            }
        }

        

        public static Task<List<UserRow>> ListTeachersAsync()
        {
            return ListUsersAsync(Role.Teacher);
        }

        

        public static async Task<List<ClassRow>> ListClassesAsync()
        {
            const string sql =
                "SELECT c.ClassID, c.ClassName, c.ClassCode, t.FullName, " +
                "       (SELECT COUNT(*) FROM SIKAI.ClassEnrollments e WHERE e.ClassID = c.ClassID) " +
                "FROM SIKAI.SikaiClasses c LEFT JOIN SIKAI.Teacher t ON t.TeacherID = c.TeacherID " +
                "ORDER BY c.ClassID DESC";
            var list = new List<ClassRow>();

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new ClassRow
                    {
                        Id = Convert.ToInt64(reader.GetValue(0)),
                        ClassName = reader.GetString(1),
                        ClassCode = reader.GetString(2),
                        TeacherName = reader.IsDBNull(3) ? "Not assigned" : reader.GetString(3),
                        StudentCount = Convert.ToInt32(reader.GetValue(4))
                    });
                }
            }
            return list;
        }

        public static async Task CreateClassAsync(string className, string classCode, long teacherId)
        {
            
            const string sql =
                "INSERT INTO SIKAI.SikaiClasses (ClassID, ClassCode, ClassName, TeacherID) " +
                "VALUES (SIKAI.class_seq.NEXTVAL, :ccode, :cname, :tid)";

            using (OracleConnection conn = await OpenAsync())
            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("ccode", classCode));
                cmd.Parameters.Add(new OracleParameter("cname", className));
                cmd.Parameters.Add(new OracleParameter("tid", teacherId));
                await cmd.ExecuteNonQueryAsync();
            }
        }

        

        public static string Describe(Exception ex, string duplicateMessage)
        {
            var ora = ex as OracleException;
            if (ora == null)
                return ex.Message;

            switch (ora.Number)
            {
                case 1:
                    return duplicateMessage;
                case 942:
                    return "A table is missing. Run admin_setup.sql (and your table script) as SIKAI.";
                case 2289:
                    return "A sequence is missing. Run admin_setup.sql as SIKAI.";
                case 2291:
                    return "That teacher no longer exists. Refresh and try again.";
                case 2292:
                    return "This account can't be removed because it has linked records " +
                           "(classes, enrollments, materials, messages). Remove those first.";
                default:
                    return "Database error ORA-" + ora.Number.ToString("00000") + ": " + ora.Message;
            }
        }
    }
}
