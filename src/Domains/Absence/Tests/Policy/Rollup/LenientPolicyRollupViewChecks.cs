namespace Acme.Workforce.Absence.Tests.Policy.Rollup;

using Acme.Workforce.Absence.Policy.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientPolicyRollupViewChecks
{
    public static void LenientPolicyRollupViewShowsTheStatusInCapitals()
    {
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
        var view = LenientPolicyRollupView.ToLenientPolicyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPolicyRollupView.ToLenientPolicyRollupViews(new List<LenientPolicyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPolicyRollupViewShowsTheStatusInCapitals();
    }
}
