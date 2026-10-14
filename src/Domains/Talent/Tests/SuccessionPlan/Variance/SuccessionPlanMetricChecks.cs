namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanMetricChecks
{
    public static void SuccessionPlanMetricStartsInCreated()
    {
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("code-339", "title-542");
        Check.Equal(item.Status, SuccessionPlanMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanMetricMovesToValidated()
    {
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("code-339", "title-542");
        try
        {
            item.MoveTo(SuccessionPlanMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanMetricStatus.Validated, "status");
    }

    public static void SuccessionPlanMetricRejectsMovesFromRetired()
    {
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("code-339", "title-542");
        item.Status = SuccessionPlanMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanMetricStartsInCreated();
        SuccessionPlanMetricMovesToValidated();
        SuccessionPlanMetricRejectsMovesFromRetired();
    }
}
