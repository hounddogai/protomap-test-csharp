namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped offboarding slot.</summary>
public sealed record CappedOffboardingSlotView(string Code, string TitleText, string Notes, string PhoneText, string BatchSize, string ExpiresAt, string LocaleText, string Name, string Location, string Badge, string StatusLabel)
{
    public static CappedOffboardingSlotView ToCappedOffboardingSlotView(CappedOffboardingSlot item)
    {
        return new CappedOffboardingSlotView(
            item.Code,
            item.Title,
            Text.Truncate(item.Notes, 80),
            Text.Truncate(item.Phone, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Name,
            item.Location,
            Text.Truncate(item.Badge, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedOffboardingSlotView> ToCappedOffboardingSlotViews(List<CappedOffboardingSlot> items)
    {
        var views = new List<CappedOffboardingSlotView>();
        foreach (var item in items)
        {
            views.Add(ToCappedOffboardingSlotView(item));
        }
        return views;
    }

    public static double SumCappedOffboardingSlotView(CappedOffboardingSlotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedExpiresAt;
    }
}
