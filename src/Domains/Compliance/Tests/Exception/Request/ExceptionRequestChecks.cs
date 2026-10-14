namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionRequestChecks
{
    public static void ExceptionRequestStartsInCreated()
    {
        var item = ExceptionRequest.NewExceptionRequest("code-466", "label-110");
        Check.Equal(item.Status, ExceptionRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionRequestMovesToValidated()
    {
        var item = ExceptionRequest.NewExceptionRequest("code-466", "label-110");
        try
        {
            item.MoveTo(ExceptionRequestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionRequestStatus.Validated, "status");
    }

    public static void ExceptionRequestRejectsMovesFromRetired()
    {
        var item = ExceptionRequest.NewExceptionRequest("code-466", "label-110");
        item.Status = ExceptionRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(ExceptionRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        ExceptionRequestStartsInCreated();
        ExceptionRequestMovesToValidated();
        ExceptionRequestRejectsMovesFromRetired();
    }
}
