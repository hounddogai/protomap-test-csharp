namespace Acme.Workforce.Assets.Laptop.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending laptop target.</summary>
public sealed record PendingLaptopTargetView(string CodeText, string Label, string BatchSize, string Visible, string Archived, string ContactEmail, string ExpiresAt, string ReviewerText, string Ceiling, string PhoneText, string StatusLabel)
{
    public static PendingLaptopTargetView ToPendingLaptopTargetView(PendingLaptopTarget item)
    {
        return new PendingLaptopTargetView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Archived}",
            Text.Truncate(item.ContactEmail, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingLaptopTargetView> ToPendingLaptopTargetViews(List<PendingLaptopTarget> items)
    {
        var views = new List<PendingLaptopTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPendingLaptopTargetView(item));
        }
        return views;
    }

    public static double SumPendingLaptopTargetView(PendingLaptopTargetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedExpiresAt + parsedCeiling;
    }
}
