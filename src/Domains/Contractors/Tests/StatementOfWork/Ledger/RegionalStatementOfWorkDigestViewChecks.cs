namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestViewChecks
{
    public static void RegionalStatementOfWorkDigestViewShowsTheStatusInCapitals()
    {
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
        var view = RegionalStatementOfWorkDigestView.ToRegionalStatementOfWorkDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalStatementOfWorkDigestView.ToRegionalStatementOfWorkDigestViews(new List<RegionalStatementOfWorkDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkDigestViewShowsTheStatusInCapitals();
    }
}
