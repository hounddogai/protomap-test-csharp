namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientPositionAuditViewChecks
{
    public static void LenientPositionAuditViewShowsTheStatusInCapitals()
    {
        var item = LenientPositionAudit.NewLenientPositionAudit("code-206", "label-518");
        var view = LenientPositionAuditView.ToLenientPositionAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPositionAuditView.ToLenientPositionAuditViews(new List<LenientPositionAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPositionAuditViewShowsTheStatusInCapitals();
    }
}
