namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkThresholdViewChecks
{
    public static void StatementOfWorkThresholdViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("code-413", "label-101");
        var view = StatementOfWorkThresholdView.ToStatementOfWorkThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkThresholdView.ToStatementOfWorkThresholdViews(new List<StatementOfWorkThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkThresholdViewShowsTheStatusInCapitals();
    }
}
