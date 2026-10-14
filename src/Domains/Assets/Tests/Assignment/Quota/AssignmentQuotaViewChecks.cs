namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentQuotaViewChecks
{
    public static void AssignmentQuotaViewShowsTheStatusInCapitals()
    {
        var item = AssignmentQuota.NewAssignmentQuota("code-466", "label-77");
        var view = AssignmentQuotaView.ToAssignmentQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentQuotaView.ToAssignmentQuotaViews(new List<AssignmentQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentQuotaViewShowsTheStatusInCapitals();
    }
}
