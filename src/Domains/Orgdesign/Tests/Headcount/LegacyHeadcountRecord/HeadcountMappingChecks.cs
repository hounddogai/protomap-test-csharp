namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class HeadcountMappingChecks
{
    public static void HeadcountMappingStartsInNew()
    {
        var item = HeadcountMapping.NewHeadcountMapping("code-972", "label-124");
        Check.Equal(item.Status, HeadcountMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountMappingMovesToRunning()
    {
        var item = HeadcountMapping.NewHeadcountMapping("code-972", "label-124");
        try
        {
            item.MoveTo(HeadcountMappingStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountMappingStatus.Running, "status");
    }

    public static void HeadcountMappingRejectsMovesFromFailed()
    {
        var item = HeadcountMapping.NewHeadcountMapping("code-972", "label-124");
        item.Status = HeadcountMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(HeadcountMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        HeadcountMappingStartsInNew();
        HeadcountMappingMovesToRunning();
        HeadcountMappingRejectsMovesFromFailed();
    }
}
