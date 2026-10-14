namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class GarnishmentPreferenceChecks
{
    public static void GarnishmentPreferenceStartsInDraft()
    {
        var item = GarnishmentPreference.NewGarnishmentPreference("code-636", "title-463");
        Check.Equal(item.Status, GarnishmentPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentPreferenceMovesToActive()
    {
        var item = GarnishmentPreference.NewGarnishmentPreference("code-636", "title-463");
        try
        {
            item.MoveTo(GarnishmentPreferenceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentPreferenceStatus.Active, "status");
    }

    public static void GarnishmentPreferenceRejectsMovesFromClosed()
    {
        var item = GarnishmentPreference.NewGarnishmentPreference("code-636", "title-463");
        item.Status = GarnishmentPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(GarnishmentPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GarnishmentPreferenceStartsInDraft();
        GarnishmentPreferenceMovesToActive();
        GarnishmentPreferenceRejectsMovesFromClosed();
    }
}
