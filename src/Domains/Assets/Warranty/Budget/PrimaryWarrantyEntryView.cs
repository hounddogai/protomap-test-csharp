namespace Acme.Workforce.Assets.Warranty.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary warranty entry.</summary>
public sealed record PrimaryWarrantyEntryView(string Code, string Title, string Checksum, string ExternalRefText, string Segment, string Quantity, string AmountText, string Team, string Channel, string Revision, string StatusLabel)
{
    public static PrimaryWarrantyEntryView ToPrimaryWarrantyEntryView(PrimaryWarrantyEntry item)
    {
        return new PrimaryWarrantyEntryView(
            item.Code,
            item.Title,
            item.Checksum,
            Text.Truncate(item.ExternalRef, 80),
            item.Segment,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Channel,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryWarrantyEntryView> ToPrimaryWarrantyEntryViews(List<PrimaryWarrantyEntry> items)
    {
        var views = new List<PrimaryWarrantyEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryWarrantyEntryView(item));
        }
        return views;
    }

    public static double SumPrimaryWarrantyEntryView(PrimaryWarrantyEntryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedAmount + (double)parsedRevision;
    }
}
