namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionSegmentChecks
{
    public static void ExceptionSegmentStartsInRequested()
    {
        var item = ExceptionSegment.NewExceptionSegment("code-148", "title-601");
        Check.Equal(item.Status, ExceptionSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionSegmentMovesToRevoked()
    {
        var item = ExceptionSegment.NewExceptionSegment("code-148", "title-601");
        try
        {
            item.MoveTo(ExceptionSegmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionSegmentStatus.Revoked, "status");
    }

    public static void ExceptionSegmentRejectsMovesFromExpired()
    {
        var item = ExceptionSegment.NewExceptionSegment("code-148", "title-601");
        item.Status = ExceptionSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(ExceptionSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ExceptionSegmentStartsInRequested();
        ExceptionSegmentMovesToRevoked();
        ExceptionSegmentRejectsMovesFromExpired();
    }
}
