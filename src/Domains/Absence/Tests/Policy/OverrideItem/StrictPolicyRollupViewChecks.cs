namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPolicyRollupViewChecks
{
    public static void StrictPolicyRollupViewShowsTheStatusInCapitals()
    {
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
        var view = StrictPolicyRollupView.ToStrictPolicyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPolicyRollupView.ToStrictPolicyRollupViews(new List<StrictPolicyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPolicyRollupViewShowsTheStatusInCapitals();
    }
}
