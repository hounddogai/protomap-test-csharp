namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedOffboardingThresholdChecks
{
    public static void WeightedOffboardingThresholdStartsInDraft()
    {
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("code-330", "label-967");
        Check.Equal(item.Status, WeightedOffboardingThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedOffboardingThresholdMovesToActive()
    {
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("code-330", "label-967");
        try
        {
            item.MoveTo(WeightedOffboardingThresholdStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedOffboardingThresholdStatus.Active, "status");
    }

    public static void WeightedOffboardingThresholdRejectsMovesFromClosed()
    {
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("code-330", "label-967");
        item.Status = WeightedOffboardingThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedOffboardingThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedOffboardingThresholdStartsInDraft();
        WeightedOffboardingThresholdMovesToActive();
        WeightedOffboardingThresholdRejectsMovesFromClosed();
    }
}
