namespace Acme.Workforce.Payrollops.Tests.TaxForm.Slot;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TaxFormSlotViewChecks
{
    public static void TaxFormSlotViewShowsTheStatusInCapitals()
    {
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
        var view = TaxFormSlotView.ToTaxFormSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormSlotView.ToTaxFormSlotViews(new List<TaxFormSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormSlotViewShowsTheStatusInCapitals();
    }
}
