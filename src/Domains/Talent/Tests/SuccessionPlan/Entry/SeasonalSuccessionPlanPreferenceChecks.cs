namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Entry;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanPreferenceChecks
{
    public static void SeasonalSuccessionPlanPreferenceStartsInDraft()
    {
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("code-766", "title-55");
        Check.Equal(item.Status, SeasonalSuccessionPlanPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSuccessionPlanPreferenceMovesToSuspended()
    {
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("code-766", "title-55");
        try
        {
            item.MoveTo(SeasonalSuccessionPlanPreferenceStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSuccessionPlanPreferenceStatus.Suspended, "status");
    }

    public static void SeasonalSuccessionPlanPreferenceRejectsMovesFromClosed()
    {
        var item = SeasonalSuccessionPlanPreference.NewSeasonalSuccessionPlanPreference("code-766", "title-55");
        item.Status = SeasonalSuccessionPlanPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalSuccessionPlanPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanPreferenceStartsInDraft();
        SeasonalSuccessionPlanPreferenceMovesToSuspended();
        SeasonalSuccessionPlanPreferenceRejectsMovesFromClosed();
    }
}
