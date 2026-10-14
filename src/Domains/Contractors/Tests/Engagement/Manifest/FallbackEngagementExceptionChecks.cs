namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackEngagementExceptionChecks
{
    public static void FallbackEngagementExceptionStartsInCreated()
    {
        var item = FallbackEngagementException.NewFallbackEngagementException("code-508", "label-82");
        Check.Equal(item.Status, FallbackEngagementExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackEngagementExceptionMovesToValidated()
    {
        var item = FallbackEngagementException.NewFallbackEngagementException("code-508", "label-82");
        try
        {
            item.MoveTo(FallbackEngagementExceptionStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackEngagementExceptionStatus.Validated, "status");
    }

    public static void FallbackEngagementExceptionRejectsMovesFromRetired()
    {
        var item = FallbackEngagementException.NewFallbackEngagementException("code-508", "label-82");
        item.Status = FallbackEngagementExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackEngagementExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackEngagementExceptionStartsInCreated();
        FallbackEngagementExceptionMovesToValidated();
        FallbackEngagementExceptionRejectsMovesFromRetired();
    }
}
