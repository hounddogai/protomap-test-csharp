namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientSpanRollupViewChecks
{
    public static void LenientSpanRollupViewShowsTheStatusInCapitals()
    {
        var item = LenientSpanRollup.NewLenientSpanRollup("code-512", "name-422");
        var view = LenientSpanRollupView.ToLenientSpanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSpanRollupView.ToLenientSpanRollupViews(new List<LenientSpanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSpanRollupViewShowsTheStatusInCapitals();
    }
}
