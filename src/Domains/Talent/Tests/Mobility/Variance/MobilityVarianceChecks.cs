namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class MobilityVarianceChecks
{
    public static void MobilityVarianceStartsInOpen()
    {
        var item = MobilityVariance.NewMobilityVariance("code-400", "title-247");
        Check.Equal(item.Status, MobilityVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityVarianceMovesToArchived()
    {
        var item = MobilityVariance.NewMobilityVariance("code-400", "title-247");
        try
        {
            item.MoveTo(MobilityVarianceStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityVarianceStatus.Archived, "status");
    }

    public static void MobilityVarianceRejectsMovesFromArchived()
    {
        var item = MobilityVariance.NewMobilityVariance("code-400", "title-247");
        item.Status = MobilityVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityVarianceStartsInOpen();
        MobilityVarianceMovesToArchived();
        MobilityVarianceRejectsMovesFromArchived();
    }
}
