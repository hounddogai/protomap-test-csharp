namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSpanRecordChecks
{
    public static void LenientSpanRecordStartsInCreated()
    {
        var item = LenientSpanRecord.NewLenientSpanRecord("code-724", "label-700");
        Check.Equal(item.Status, LenientSpanRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSpanRecordMovesToPublished()
    {
        var item = LenientSpanRecord.NewLenientSpanRecord("code-724", "label-700");
        try
        {
            item.MoveTo(LenientSpanRecordStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSpanRecordStatus.Published, "status");
    }

    public static void LenientSpanRecordRejectsMovesFromRetired()
    {
        var item = LenientSpanRecord.NewLenientSpanRecord("code-724", "label-700");
        item.Status = LenientSpanRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientSpanRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientSpanRecordStartsInCreated();
        LenientSpanRecordMovesToPublished();
        LenientSpanRecordRejectsMovesFromRetired();
    }
}
