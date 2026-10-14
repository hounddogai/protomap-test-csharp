namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class RollingCostModelPreferenceChecks
{
    public static void RollingCostModelPreferenceStartsInDraft()
    {
        var item = RollingCostModelPreference.NewRollingCostModelPreference("code-294", "title-981");
        Check.Equal(item.Status, RollingCostModelPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingCostModelPreferenceMovesToSuspended()
    {
        var item = RollingCostModelPreference.NewRollingCostModelPreference("code-294", "title-981");
        try
        {
            item.MoveTo(RollingCostModelPreferenceStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingCostModelPreferenceStatus.Suspended, "status");
    }

    public static void RollingCostModelPreferenceRejectsMovesFromClosed()
    {
        var item = RollingCostModelPreference.NewRollingCostModelPreference("code-294", "title-981");
        item.Status = RollingCostModelPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingCostModelPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingCostModelPreferenceStartsInDraft();
        RollingCostModelPreferenceMovesToSuspended();
        RollingCostModelPreferenceRejectsMovesFromClosed();
    }
}
