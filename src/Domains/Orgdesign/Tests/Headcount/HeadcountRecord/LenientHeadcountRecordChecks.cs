namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LenientHeadcountRecordChecks
{
    public static void LenientHeadcountRecordStartsInIdle()
    {
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("code-360", "label-422");
        Check.Equal(item.Status, LenientHeadcountRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientHeadcountRecordMovesToDraining()
    {
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("code-360", "label-422");
        try
        {
            item.MoveTo(LenientHeadcountRecordStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientHeadcountRecordStatus.Draining, "status");
    }

    public static void LenientHeadcountRecordRejectsMovesFromStopped()
    {
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("code-360", "label-422");
        item.Status = LenientHeadcountRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientHeadcountRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientHeadcountRecordStartsInIdle();
        LenientHeadcountRecordMovesToDraining();
        LenientHeadcountRecordRejectsMovesFromStopped();
    }
}
