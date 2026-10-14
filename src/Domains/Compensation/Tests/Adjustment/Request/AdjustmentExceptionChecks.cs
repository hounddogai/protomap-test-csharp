namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentExceptionChecks
{
    public static void AdjustmentExceptionStartsInCreated()
    {
        var item = AdjustmentException.NewAdjustmentException("code-399", "title-95");
        Check.Equal(item.Status, AdjustmentExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentExceptionMovesToValidated()
    {
        var item = AdjustmentException.NewAdjustmentException("code-399", "title-95");
        try
        {
            item.MoveTo(AdjustmentExceptionStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentExceptionStatus.Validated, "status");
    }

    public static void AdjustmentExceptionRejectsMovesFromRetired()
    {
        var item = AdjustmentException.NewAdjustmentException("code-399", "title-95");
        item.Status = AdjustmentExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(AdjustmentExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdjustmentExceptionStartsInCreated();
        AdjustmentExceptionMovesToValidated();
        AdjustmentExceptionRejectsMovesFromRetired();
    }
}
