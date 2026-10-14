namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityScoreChecks
{
    public static void MobilityScoreStartsInNew()
    {
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
        Check.Equal(item.Status, MobilityScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityScoreMovesToDone()
    {
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
        try
        {
            item.MoveTo(MobilityScoreStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityScoreStatus.Done, "status");
    }

    public static void MobilityScoreRejectsMovesFromFailed()
    {
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
        item.Status = MobilityScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityScoreStartsInNew();
        MobilityScoreMovesToDone();
        MobilityScoreRejectsMovesFromFailed();
    }
}
