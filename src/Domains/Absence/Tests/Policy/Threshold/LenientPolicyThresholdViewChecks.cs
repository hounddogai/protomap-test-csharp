namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdViewChecks
{
    public static void LenientPolicyThresholdViewShowsTheStatusInCapitals()
    {
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("code-693", "name-137");
        var view = LenientPolicyThresholdView.ToLenientPolicyThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPolicyThresholdView.ToLenientPolicyThresholdViews(new List<LenientPolicyThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPolicyThresholdViewShowsTheStatusInCapitals();
    }
}
