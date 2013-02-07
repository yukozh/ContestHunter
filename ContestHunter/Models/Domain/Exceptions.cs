using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class PasswordMismatchException : Exception
    {
    }

    public class DatabaseException : Exception
    {
    }

    public class UserNotFoundException : Exception
    {
    }

    public class UndefineException : Exception
    {
    }

    public class BadTokenException : Exception
    {
    }

}