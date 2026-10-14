namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountVarianceChecks
{
    public static void HeadcountVarianceStartsInNew()
    {
        var item = HeadcountVariance.NewHeadcountVariance("code-765", "label-685");
        Check.Equal(item.Status, HeadcountVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountVarianceMovesToRunning()
    {
        var item = HeadcountVariance.NewHeadcountVariance("code-765", "label-685");
        try
        {
            item.MoveTo(HeadcountVarianceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountVarianceStatus.Running, "status");
    }

    public static void HeadcountVarianceRejectsMovesFromFailed()
    {
        var item = HeadcountVariance.NewHeadcountVariance("code-765", "label-685");
        item.Status = HeadcountVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(HeadcountVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        HeadcountVarianceStartsInNew();
        HeadcountVarianceMovesToRunning();
        HeadcountVarianceRejectsMovesFromFailed();
    }
}
