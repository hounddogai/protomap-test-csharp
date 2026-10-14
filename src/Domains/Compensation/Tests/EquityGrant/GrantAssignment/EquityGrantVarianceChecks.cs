namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class EquityGrantVarianceChecks
{
    public static void EquityGrantVarianceStartsInOpen()
    {
        var item = EquityGrantVariance.NewEquityGrantVariance("code-615", "title-587");
        Check.Equal(item.Status, EquityGrantVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantVarianceMovesToResolved()
    {
        var item = EquityGrantVariance.NewEquityGrantVariance("code-615", "title-587");
        try
        {
            item.MoveTo(EquityGrantVarianceStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantVarianceStatus.Resolved, "status");
    }

    public static void EquityGrantVarianceRejectsMovesFromArchived()
    {
        var item = EquityGrantVariance.NewEquityGrantVariance("code-615", "title-587");
        item.Status = EquityGrantVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantVarianceStartsInOpen();
        EquityGrantVarianceMovesToResolved();
        EquityGrantVarianceRejectsMovesFromArchived();
    }
}
