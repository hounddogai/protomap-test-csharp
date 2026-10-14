namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkProfileViewChecks
{
    public static void StatementOfWorkProfileViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("code-59", "label-832");
        var view = StatementOfWorkProfileView.ToStatementOfWorkProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkProfileView.ToStatementOfWorkProfileViews(new List<StatementOfWorkProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkProfileViewShowsTheStatusInCapitals();
    }
}
