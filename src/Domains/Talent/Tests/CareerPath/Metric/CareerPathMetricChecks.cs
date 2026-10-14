namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathMetricChecks
{
    public static void CareerPathMetricStartsInRequested()
    {
        var item = CareerPathMetric.NewCareerPathMetric("code-12", "label-973");
        Check.Equal(item.Status, CareerPathMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathMetricMovesToExpired()
    {
        var item = CareerPathMetric.NewCareerPathMetric("code-12", "label-973");
        try
        {
            item.MoveTo(CareerPathMetricStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathMetricStatus.Expired, "status");
    }

    public static void CareerPathMetricRejectsMovesFromExpired()
    {
        var item = CareerPathMetric.NewCareerPathMetric("code-12", "label-973");
        item.Status = CareerPathMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathMetricStartsInRequested();
        CareerPathMetricMovesToExpired();
        CareerPathMetricRejectsMovesFromExpired();
    }
}
