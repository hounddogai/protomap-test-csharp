namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceChecks
{
    public static void SeasonalCostModelPreferenceStartsInPlanned()
    {
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
        Check.Equal(item.Status, SeasonalCostModelPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalCostModelPreferenceMovesToCancelled()
    {
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
        try
        {
            item.MoveTo(SeasonalCostModelPreferenceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalCostModelPreferenceStatus.Cancelled, "status");
    }

    public static void SeasonalCostModelPreferenceRejectsMovesFromCancelled()
    {
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
        item.Status = SeasonalCostModelPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalCostModelPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalCostModelPreferenceStartsInPlanned();
        SeasonalCostModelPreferenceMovesToCancelled();
        SeasonalCostModelPreferenceRejectsMovesFromCancelled();
    }
}
