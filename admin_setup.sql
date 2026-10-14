



CREATE TABLE AdminAccount (
    AdminID   NUMBER PRIMARY KEY,
    FullName  VARCHAR2(100) NOT NULL,
    Email     VARCHAR2(100) UNIQUE NOT NULL,
    Password  VARCHAR2(255) NOT NULL
);

CREATE SEQUENCE admin_seq START WITH 1 INCREMENT BY 1;




CREATE SEQUENCE class_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE enrollment_seq START WITH 1 INCREMENT BY 1;

CREATE TABLE SikaiClasses (
    ClassID    NUMBER PRIMARY KEY,
    ClassCode  VARCHAR2(50)  NOT NULL UNIQUE,
    ClassName  VARCHAR2(200) NOT NULL,
    TeacherID  NUMBER NOT NULL
);

CREATE TABLE ClassEnrollments (
    EnrollmentID NUMBER PRIMARY KEY,
    ClassID      NUMBER NOT NULL,
    StudentID    NUMBER NOT NULL,
    EnrolledAt   DATE DEFAULT SYSDATE,
    CONSTRAINT fk_enroll_class   FOREIGN KEY (ClassID)   REFERENCES SikaiClasses(ClassID),
    CONSTRAINT fk_enroll_student FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    CONSTRAINT uq_class_student  UNIQUE (ClassID, StudentID)
);



ALTER TABLE Student ADD (SecurityQuestion VARCHAR2(200), SecurityAnswer VARCHAR2(255));
ALTER TABLE Teacher ADD (SecurityQuestion VARCHAR2(200), SecurityAnswer VARCHAR2(255));



DELETE FROM AdminAccount WHERE Email = 'admin@sikai.test';

INSERT INTO AdminAccount (AdminID, FullName, Email, Password)
VALUES (admin_seq.NEXTVAL, 'Site Admin', 'admin@sikai.test',
        'Y7cHyAsZ9wkuc/kcAs7oUA==:hWRZSV6doT4sPi9vzzWXHm2rtDuRZAWodt1AQ2aDftw=');

COMMIT;
