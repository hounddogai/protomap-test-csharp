namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordChecks
{
    public static void AdaptiveHeadcountRecordStartsInRequested()
    {
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
        Check.Equal(item.Status, AdaptiveHeadcountRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveHeadcountRecordMovesToExpired()
    {
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
        try
        {
            item.MoveTo(AdaptiveHeadcountRecordStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveHeadcountRecordStatus.Expired, "status");
    }

    public static void AdaptiveHeadcountRecordRejectsMovesFromExpired()
    {
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
        item.Status = AdaptiveHeadcountRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveHeadcountRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountRecordStartsInRequested();
        AdaptiveHeadcountRecordMovesToExpired();
        AdaptiveHeadcountRecordRejectsMovesFromExpired();
    }
}
