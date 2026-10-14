namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricChecks
{
    public static void LicenseSeatMetricStartsInNew()
    {
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
        Check.Equal(item.Status, LicenseSeatMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatMetricMovesToRunning()
    {
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
        try
        {
            item.MoveTo(LicenseSeatMetricStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatMetricStatus.Running, "status");
    }

    public static void LicenseSeatMetricRejectsMovesFromFailed()
    {
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
        item.Status = LicenseSeatMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(LicenseSeatMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        LicenseSeatMetricStartsInNew();
        LicenseSeatMetricMovesToRunning();
        LicenseSeatMetricRejectsMovesFromFailed();
    }
}
