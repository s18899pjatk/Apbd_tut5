CREATE PROCEDURE PromoteStudents(@StudyName VARCHAR(100), @SEMESTER INT)
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
DECLARE @LastIdEnrollment INT = (SELECT MAX(IdEnrollment) FROM Enrollment);
--if no such enrollment - adding it to the table
IF @IdNextEnrollment IS NULL
BEGIN
	INSERT INTO Enrollment VALUES (@LastIdEnrollment+1,@SEMESTER+1,@IdStudy,SYSDATETIME());
	Set @IdNextEnrollment = @LastIdEnrollment+1;
END
DECLARE @PrevEnrollment INT = (SELECT DISTINCT e.IdEnrollment From Student s join Enrollment e on s.IdEnrollment=e.IdEnrollment where e.Semester=@SEMESTER AND e.IdStudy = @IdStudy);
UPDATE Student 
	SET IdEnrollment=@IdNextEnrollment
	WHERE IdEnrollment=@PrevEnrollment;
COMMIT
END;

-- FOR checking
EXECUTE PromoteStudents 'Computer science',1;
DELETE  Enrollment where IdEnrollment=4;
UPDATE Student 
SET IdEnrollment=3
WHERE IdEnrollment=4;
--SELECT Distinct s.FirstName From Student s join Enrollment e on s.IdEnrollment=e.IdEnrollment where e.Semester=1 AND e.IdStudy=1;
SELECT * FROM Studies;
SELECT * FROM Student;
Select * FROM Enrollment;
DELETE Student where IndexNumber='s12345';