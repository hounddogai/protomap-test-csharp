namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreChecks
{
    public static void RetroPayScoreStartsInOpen()
    {
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
        Check.Equal(item.Status, RetroPayScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayScoreMovesToReopened()
    {
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
        try
        {
            item.MoveTo(RetroPayScoreStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayScoreStatus.Reopened, "status");
    }

    public static void RetroPayScoreRejectsMovesFromArchived()
    {
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
        item.Status = RetroPayScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(RetroPayScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        RetroPayScoreStartsInOpen();
        RetroPayScoreMovesToReopened();
        RetroPayScoreRejectsMovesFromArchived();
    }
}
