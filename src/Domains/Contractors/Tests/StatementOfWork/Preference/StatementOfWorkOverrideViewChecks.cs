namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideViewChecks
{
    public static void StatementOfWorkOverrideViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
        var view = StatementOfWorkOverrideView.ToStatementOfWorkOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkOverrideView.ToStatementOfWorkOverrideViews(new List<StatementOfWorkOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkOverrideViewShowsTheStatusInCapitals();
    }
}
