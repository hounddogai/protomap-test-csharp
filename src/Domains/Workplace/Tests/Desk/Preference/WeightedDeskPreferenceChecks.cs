namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceChecks
{
    public static void WeightedDeskPreferenceStartsInPlanned()
    {
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
        Check.Equal(item.Status, WeightedDeskPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedDeskPreferenceMovesToScheduled()
    {
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
        try
        {
            item.MoveTo(WeightedDeskPreferenceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedDeskPreferenceStatus.Scheduled, "status");
    }

    public static void WeightedDeskPreferenceRejectsMovesFromCancelled()
    {
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
        item.Status = WeightedDeskPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedDeskPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedDeskPreferenceStartsInPlanned();
        WeightedDeskPreferenceMovesToScheduled();
        WeightedDeskPreferenceRejectsMovesFromCancelled();
    }
}
