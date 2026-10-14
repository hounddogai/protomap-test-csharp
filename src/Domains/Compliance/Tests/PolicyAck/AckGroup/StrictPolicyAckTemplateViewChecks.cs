namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.PolicyAck.AckGroup;

/// <summary>Checks of ack group code, run by the project's runner.</summary>
public static class StrictPolicyAckTemplateViewChecks
{
    public static void StrictPolicyAckTemplateViewShowsTheStatusInCapitals()
    {
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("code-586", "label-604");
        var view = StrictPolicyAckTemplateView.ToStrictPolicyAckTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPolicyAckTemplateView.ToStrictPolicyAckTemplateViews(new List<StrictPolicyAckTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPolicyAckTemplateViewShowsTheStatusInCapitals();
    }
}
