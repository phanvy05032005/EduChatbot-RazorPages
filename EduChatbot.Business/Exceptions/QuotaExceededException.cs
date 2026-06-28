using System;

namespace EduChatbot.Business.Exceptions;

public class QuotaExceededException : InvalidOperationException
{
    public QuotaExceededException(string message) : base(message)
    {
    }
}
