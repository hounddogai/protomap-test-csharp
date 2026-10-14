namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineViewChecks
{
    public static void CappedWarrantyBaselineViewShowsTheStatusInCapitals()
    {
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
        var view = CappedWarrantyBaselineView.ToCappedWarrantyBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedWarrantyBaselineView.ToCappedWarrantyBaselineViews(new List<CappedWarrantyBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedWarrantyBaselineViewShowsTheStatusInCapitals();
    }
}
