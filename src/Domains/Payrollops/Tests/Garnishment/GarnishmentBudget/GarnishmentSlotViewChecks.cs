namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class GarnishmentSlotViewChecks
{
    public static void GarnishmentSlotViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentSlot.NewGarnishmentSlot("code-693", "title-683");
        var view = GarnishmentSlotView.ToGarnishmentSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentSlotView.ToGarnishmentSlotViews(new List<GarnishmentSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentSlotViewShowsTheStatusInCapitals();
    }
}
