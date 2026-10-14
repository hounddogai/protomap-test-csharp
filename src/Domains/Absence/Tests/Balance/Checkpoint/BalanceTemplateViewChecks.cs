namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BalanceTemplateViewChecks
{
    public static void BalanceTemplateViewShowsTheStatusInCapitals()
    {
        var item = BalanceTemplate.NewBalanceTemplate("code-301", "name-924");
        var view = BalanceTemplateView.ToBalanceTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceTemplateView.ToBalanceTemplateViews(new List<BalanceTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceTemplateViewShowsTheStatusInCapitals();
    }
}
