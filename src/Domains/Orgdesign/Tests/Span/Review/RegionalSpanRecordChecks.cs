namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalSpanRecordChecks
{
    public static void RegionalSpanRecordStartsInCreated()
    {
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
        Check.Equal(item.Status, RegionalSpanRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSpanRecordMovesToValidated()
    {
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
        try
        {
            item.MoveTo(RegionalSpanRecordStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSpanRecordStatus.Validated, "status");
    }

    public static void RegionalSpanRecordRejectsMovesFromRetired()
    {
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
        item.Status = RegionalSpanRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalSpanRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalSpanRecordStartsInCreated();
        RegionalSpanRecordMovesToValidated();
        RegionalSpanRecordRejectsMovesFromRetired();
    }
}
