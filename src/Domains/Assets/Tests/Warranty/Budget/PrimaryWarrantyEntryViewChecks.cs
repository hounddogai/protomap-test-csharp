namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryWarrantyEntryViewChecks
{
    public static void PrimaryWarrantyEntryViewShowsTheStatusInCapitals()
    {
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("code-601", "title-474");
        var view = PrimaryWarrantyEntryView.ToPrimaryWarrantyEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryWarrantyEntryView.ToPrimaryWarrantyEntryViews(new List<PrimaryWarrantyEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryWarrantyEntryViewShowsTheStatusInCapitals();
    }
}
