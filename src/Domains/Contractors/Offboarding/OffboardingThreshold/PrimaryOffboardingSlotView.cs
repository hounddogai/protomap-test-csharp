namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using System.Globalization;

/// <summary>How screens and exports show a primary offboarding slot.</summary>
public sealed record PrimaryOffboardingSlotView(string Code, string Name, string Currency, string Archived, string Discount, string StatusLabel)
{
    public static PrimaryOffboardingSlotView ToPrimaryOffboardingSlotView(PrimaryOffboardingSlot item)
    {
        return new PrimaryOffboardingSlotView(
            item.Code,
            item.Name,
            item.Currency,
            $"{item.Archived}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryOffboardingSlotView> ToPrimaryOffboardingSlotViews(List<PrimaryOffboardingSlot> items)
    {
        var views = new List<PrimaryOffboardingSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryOffboardingSlotView(item));
        }
        return views;
    }

    public static double SumPrimaryOffboardingSlotView(PrimaryOffboardingSlotView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
