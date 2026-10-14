namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkThresholdChecks
{
    public static void StatementOfWorkThresholdStartsInOpen()
    {
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("code-413", "label-101");
        Check.Equal(item.Status, StatementOfWorkThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkThresholdMovesToInReview()
    {
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("code-413", "label-101");
        try
        {
            item.MoveTo(StatementOfWorkThresholdStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkThresholdStatus.InReview, "status");
    }

    public static void StatementOfWorkThresholdRejectsMovesFromArchived()
    {
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("code-413", "label-101");
        item.Status = StatementOfWorkThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(StatementOfWorkThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkThresholdStartsInOpen();
        StatementOfWorkThresholdMovesToInReview();
        StatementOfWorkThresholdRejectsMovesFromArchived();
    }
}
