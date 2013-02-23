using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class PasswordMismatchException : Exception
    {
    }

    public class EmailMismatchException : Exception 
    {
    }

    public class UserNotFoundException : Exception
    {
    }

    public class UndefinedException : Exception
    {
    }

    public class BadTokenException : Exception
    {
    }

    public class PermissionDeniedException : Exception
    {
    }

    public class UserNotLoginException : Exception
    {
    }

    public class AlreadyAttendedContestException : Exception
    {
    }

    public class NotAttendedContestException : Exception
    {
    }

    public class ContestStartedException : Exception
    {
    }

    public class ContestNotStartedException : Exception
    {
    }

    public class ProblemNotFoundException : Exception
    {
    }

    public class GroupNotFoundException : Exception
    {
    }

    public class ContestNotEndedException : Exception
    {
    }

    public class TestCaseNotFoundException : Exception
    {
    }

    public class ContestNotFoundException : Exception
    {
    }

    public class RecordNotFoundException : Exception
    {
    }

    public class ContestTypeMismatchException : Exception
    {
    }

    public class AttendedNotVirtualException : Exception
    {
    }

    public class ContestEndedException : Exception
    {
    }

    public class ProblemNotLockedException : Exception
    {
    }

    public class AttendedNotNormalException : Exception
    {
    }

    public class ProblemLockedException : Exception
    {
    }

    public class ProblemNameExistedException : Exception
    {
    }

    public class RecordStatusMismatchException : Exception
    {
    }

    public class NoRecordWaitingException : Exception
    {
    }

    public class NoHuntWaitingException : Exception
    {
    }

    public class ContestNameExistedException : Exception
    {
    }

    public class ProblemNotPassedException : Exception
    {
    }

}