namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveRateTemplateChecks
{
    public static void AdaptiveRateTemplateStartsInIdle()
    {
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("code-707", "label-144");
        Check.Equal(item.Status, AdaptiveRateTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveRateTemplateMovesToWarming()
    {
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("code-707", "label-144");
        try
        {
            item.MoveTo(AdaptiveRateTemplateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveRateTemplateStatus.Warming, "status");
    }

    public static void AdaptiveRateTemplateRejectsMovesFromStopped()
    {
        var item = AdaptiveRateTemplate.NewAdaptiveRateTemplate("code-707", "label-144");
        item.Status = AdaptiveRateTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveRateTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveRateTemplateStartsInIdle();
        AdaptiveRateTemplateMovesToWarming();
        AdaptiveRateTemplateRejectsMovesFromStopped();
    }
}
