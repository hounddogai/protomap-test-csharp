namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRecordViewChecks
{
    public static void LenientSuccessionPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("code-53", "name-23");
        var view = LenientSuccessionPlanRecordView.ToLenientSuccessionPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSuccessionPlanRecordView.ToLenientSuccessionPlanRecordViews(new List<LenientSuccessionPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanRecordViewShowsTheStatusInCapitals();
    }
}
