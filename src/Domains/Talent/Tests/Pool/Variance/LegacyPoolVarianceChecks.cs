namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyPoolVarianceChecks
{
    public static void LegacyPoolVarianceStartsInOpen()
    {
        var item = LegacyPoolVariance.NewLegacyPoolVariance("code-846", "title-32");
        Check.Equal(item.Status, LegacyPoolVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPoolVarianceMovesToResolved()
    {
        var item = LegacyPoolVariance.NewLegacyPoolVariance("code-846", "title-32");
        try
        {
            item.MoveTo(LegacyPoolVarianceStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPoolVarianceStatus.Resolved, "status");
    }

    public static void LegacyPoolVarianceRejectsMovesFromArchived()
    {
        var item = LegacyPoolVariance.NewLegacyPoolVariance("code-846", "title-32");
        item.Status = LegacyPoolVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyPoolVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyPoolVarianceStartsInOpen();
        LegacyPoolVarianceMovesToResolved();
        LegacyPoolVarianceRejectsMovesFromArchived();
    }
}
