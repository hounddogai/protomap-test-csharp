namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientPolicyAckTemplateViewChecks
{
    public static void LenientPolicyAckTemplateViewShowsTheStatusInCapitals()
    {
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("code-40", "title-134");
        var view = LenientPolicyAckTemplateView.ToLenientPolicyAckTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPolicyAckTemplateView.ToLenientPolicyAckTemplateViews(new List<LenientPolicyAckTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPolicyAckTemplateViewShowsTheStatusInCapitals();
    }
}
