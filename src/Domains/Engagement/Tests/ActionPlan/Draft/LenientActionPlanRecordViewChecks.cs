namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientActionPlanRecordViewChecks
{
    public static void LenientActionPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("code-509", "name-170");
        var view = LenientActionPlanRecordView.ToLenientActionPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientActionPlanRecordView.ToLenientActionPlanRecordViews(new List<LenientActionPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientActionPlanRecordViewShowsTheStatusInCapitals();
    }
}
