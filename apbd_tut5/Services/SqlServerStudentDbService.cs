
using apbd_tut5.DTOs.Requests;
using apbd_tut5.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tutorial5.Models;

namespace Tutorial5.Services
{
    public class SqlServerStudentDbService : IStudentsSerivceDb
    {
        private string _connString = "Data Source=db-mssql;Initial Catalog=s18899;Integrated Security=True;MultipleActiveResultSets=True;";

        public IEnumerable<Student> GetStudents()
        {
            var students = new List<Student>();
            using var sqlConnection = new SqlConnection(_connString);
            using var command = new SqlCommand();
            command.Connection = sqlConnection;
            command.CommandText = "select s.IndexNumber, s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester " +
                "from Student s " +
                "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                "join Studies st on st.IdStudy = e.IdStudy; ";
            sqlConnection.Open();
            SqlDataReader response = command.ExecuteReader();
            while (response.Read())
            {
                var st = new Student
                {
                    IndexNumber = response["IndexNumber"].ToString(),
                    FirstName = response["FirstName"].ToString(),
                    LastName = response["LastName"].ToString(),
                    Studies = response["Studies"].ToString(),
                    BirthDate = DateTime.Parse(response["BirthDate"].ToString()),
                    Semester = int.Parse(response["Semester"].ToString())

                };

                students.Add(st);
            }

            return students;
        }

        public IEnumerable<string> GetSemester(string id)
        {
            using var sqlConnection = new SqlConnection(_connString);
            using var command = new SqlCommand();
            command.Connection = sqlConnection;
            command.CommandText = "select e.Semester " +
                "from Student s " +
                "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                "where IndexNumber like @index;";
            SqlParameter par = new SqlParameter();
            par.ParameterName = "index";
            par.Value = id;
            command.Parameters.Add(par);
            sqlConnection.Open();
            SqlDataReader response = command.ExecuteReader();
            var entriesList = new List<string>();
            while (response.Read())
                entriesList.Add(response["Semester"].ToString());

            if (entriesList.Count > 0)
            {
                return entriesList;
            }
            else
            {
                return null;
            }
        }
        /*------------------------------------------------------------------ASSIGNMENT 5-------------------------------------------------------------------*/
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response = new EnrollStudentResponse();

            using (SqlConnection con = new SqlConnection(_connString))
            {
                con.Open();
                var tran = con.BeginTransaction();
                int idStudy;
                int lastIdEnrollment;
                int semester;
                try
                {
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.Transaction = tran;
                        com.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdEnrollment = (SELECT MAX(e1.IdEnrollment) From Enrollment e1); "; // looking for the last Id 
                        var dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            lastIdEnrollment = 1;
                        }
                        lastIdEnrollment = (int)dr["IdEnrollment"];
                        dr.Close();
                    }

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.Transaction = tran;
                        //checking whether we have such studies or no
                        com.CommandText = "SELECT IdStudy FROM Studies WHERE Name=@Name";
                        com.Parameters.AddWithValue("Name", request.Studies);
                        var dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            //return BadRequest("Studies does not exists");
                            return null;
                        }
                        idStudy = (int)dr["idStudy"];
                        dr.Close();

                    };
                    using (SqlCommand com1 = new SqlCommand())
                    using (SqlCommand com2 = new SqlCommand())
                    {
                        com1.Connection = con;
                        com1.Transaction = tran;
                        com2.Connection = con;
                        com2.Transaction = tran;

                        //cheking whether we have the records with semester 1
                        com1.CommandText = "SELECT Semester FROM Enrollment where Semester=1 AND IdStudy = @idStudy";
                        com1.Parameters.AddWithValue("idStudy", idStudy);
                        var dr1 = com1.ExecuteReader();
                        // if no inserting new one
                        if (!dr1.Read())
                        {
                            semester = 1;
                            DateTime now = DateTime.Now;
                            com2.CommandText = "INSERT INTO Enrollment(IdEnrollment,Semester,IdStudy,StartDate) values (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                            com2.Parameters.AddWithValue("IdEnrollment", ++lastIdEnrollment);
                            com2.Parameters.AddWithValue("Semester", semester);
                            com2.Parameters.AddWithValue("idStudy", idStudy);
                            com2.Parameters.AddWithValue("StartDate", now.Date);
                            com2.ExecuteNonQuery();
                        }
                        else semester = (int)dr1["Semester"];
                        dr1.Close();
                    };
                    using (SqlCommand com1 = new SqlCommand())
                    using (SqlCommand com2 = new SqlCommand())
                    {
                        com1.Connection = con;
                        com1.Transaction = tran;
                        com2.Connection = con;
                        com2.Transaction = tran;

                        //checking whether we already have student with the same stud number
                        com1.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber LIKE @idStudent";
                        com1.Parameters.AddWithValue("idStudent", request.IndexNumber);
                        var dr2 = com1.ExecuteReader();
                        if (dr2.Read())
                        {
                            return null;
                            // return BadRequest("Student with such ID has already exists");
                        }
                        //Insert into student
                        com2.CommandText = "INSERT INTO Student(IndexNumber,FirstName, LastName, BirthDate, IdEnrollment) Values (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                        com2.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                        com2.Parameters.AddWithValue("FirstName", request.FirstName);
                        com2.Parameters.AddWithValue("LastName", request.LastName);
                        com2.Parameters.AddWithValue("BirthDate", request.BirthDate);
                        com2.Parameters.AddWithValue("IdEnrollment", lastIdEnrollment);
                        com2.ExecuteNonQuery();
                        dr2.Close();
                    };
                    response.LastName = request.LastName;
                    response.Semester = semester;
                    tran.Commit();
                }
                catch (SqlException)
                {
                    tran.Rollback();
                }
            }
            return response;
        }




        public PromoteStudentResponse PromoteStudent(PromoteStudentRequest req)
        {
            PromoteStudentResponse response = new PromoteStudentResponse();
            using (SqlConnection con = new SqlConnection(_connString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("PromoteStudents", con))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.Add(new SqlParameter("@StudyName", req.Studies));
                    com.Parameters.Add(new SqlParameter("@SEMESTER", req.Semester));
                    com.ExecuteNonQuery();
                }

                response.Studies = req.Studies;
                response.Semester = req.Semester + 1;

            }
            return response;
        }
    }
}
