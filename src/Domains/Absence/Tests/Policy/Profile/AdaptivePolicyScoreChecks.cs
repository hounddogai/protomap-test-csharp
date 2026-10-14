namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptivePolicyScoreChecks
{
    public static void AdaptivePolicyScoreStartsInNew()
    {
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("code-465", "title-780");
        Check.Equal(item.Status, AdaptivePolicyScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyScoreMovesToDone()
    {
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("code-465", "title-780");
        try
        {
            item.MoveTo(AdaptivePolicyScoreStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyScoreStatus.Done, "status");
    }

    public static void AdaptivePolicyScoreRejectsMovesFromFailed()
    {
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("code-465", "title-780");
        item.Status = AdaptivePolicyScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptivePolicyScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyScoreStartsInNew();
        AdaptivePolicyScoreMovesToDone();
        AdaptivePolicyScoreRejectsMovesFromFailed();
    }
}
