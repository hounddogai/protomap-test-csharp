namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkDigestViewChecks
{
    public static void StatementOfWorkDigestViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
        var view = StatementOfWorkDigestView.ToStatementOfWorkDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkDigestView.ToStatementOfWorkDigestViews(new List<StatementOfWorkDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkDigestViewShowsTheStatusInCapitals();
    }
}
