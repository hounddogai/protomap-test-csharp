namespace Acme.Workforce.Assets.Warranty.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty draft.</summary>
public sealed record WarrantyDraftView(string Code, string Title, string Currency, string FloorValue, string StartsAtText, string OwnerId, string Attempts, string TenureMonths, string StatusLabel)
{
    public static WarrantyDraftView ToWarrantyDraftView(WarrantyDraft item)
    {
        return new WarrantyDraftView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Currency,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyDraftView> ToWarrantyDraftViews(List<WarrantyDraft> items)
    {
        var views = new List<WarrantyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyDraftView(item));
        }
        return views;
    }

    public static double SumWarrantyDraftView(WarrantyDraftView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedStartsAt + (double)parsedAttempts;
    }
}
