ALTER PROCEDURE PromoteStudents(@StudyName VARCHAR(100), @SEMESTER INT)
AS
BEGIN
	
BEGIN TRAN
DECLARE @IdStudy INT = (SELECT IdStudy FROM Studies WHERE NAME=@StudyName);
IF @IdStudy IS NULL 
BEGIN
	RAISERROR(400,1,1,'Studies does not exists');
END
--Find enrollment
DECLARE	@IdNextEnrollment INT = (SELECT IdEnrollment FROM Enrollment
								WHERE IdStudy=@IdStudy AND Semester=@Semester+1);
DECLARE @LastIdEnrollment INT = (SELECT MAX(IdEnrollment) FROM Enrollment
								WHERE IdStudy=@IdStudy AND Semester=@Semester);

IF @IdNextEnrollment IS NULL
BEGIN

INSERT INTO Enrollment VALUES (@LastIdEnrollment+1,@SEMESTER+1,@IdStudy,SYSDATETIME());
END

UPDATE Student 
SET IdEnrollment=@IdNextEnrollment
WHERE IdEnrollment= @LastIdEnrollment;
	COMMIT
END

SELECT * FROM Studies;
SELECT * FROM Student;
Select * FROM Enrollment;
SELECT MAX(Semester) FROM Enrollment
								WHERE IdStudy='Art';
SELECT IdEnrollment FROM Enrollment where Semester=1 AND IdStudy = 1;
SELECT * FROM Student where IndexNumber='s12345';
SELECT IdEnrollment FROM Enrollment WHERE IdEnrollment = (SELECT MAX(e1.IdEnrollment)
															From Enrollment e1);
INSERT INTO Enrollments(IdEnrollment,Semester,IdStudy,StartDate) values (3, 1, 2, '2020-04-05');
