using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class PasswordMismatchException : Exception
    {
    }

    public class EmailMismatchException : Exception { }

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
}