namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class RollingRecognitionProposalChecks
{
    public static void RollingRecognitionProposalStartsInNew()
    {
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("code-295", "label-71");
        Check.Equal(item.Status, RollingRecognitionProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRecognitionProposalMovesToRunning()
    {
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("code-295", "label-71");
        try
        {
            item.MoveTo(RollingRecognitionProposalStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRecognitionProposalStatus.Running, "status");
    }

    public static void RollingRecognitionProposalRejectsMovesFromFailed()
    {
        var item = RollingRecognitionProposal.NewRollingRecognitionProposal("code-295", "label-71");
        item.Status = RollingRecognitionProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingRecognitionProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingRecognitionProposalStartsInNew();
        RollingRecognitionProposalMovesToRunning();
        RollingRecognitionProposalRejectsMovesFromFailed();
    }
}
