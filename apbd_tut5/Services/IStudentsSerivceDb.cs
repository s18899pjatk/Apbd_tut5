using apbd_tut5.DTOs.Requests;
using apbd_tut5.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tutorial5.Models;

namespace Tutorial5.Services
{
    public interface IStudentsSerivceDb
    {
        IEnumerable<Student> GetStudents();

        IEnumerable<string> GetSemester(string id);

        EnrollStudentResponse EnrollStudent(EnrollStudentRequest req);

        PromoteStudentResponse PromoteStudent(PromoteStudentRequest req);
    }
}
