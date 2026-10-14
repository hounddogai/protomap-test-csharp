namespace Acme.Workforce.Compliance.Exception.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending exception quota.</summary>
public sealed record PendingExceptionQuotaView(string CodeText, string Name, string Quantity, string VisibleText, string Pinned, string Active, string Location, string Locale, string JobCode, string ChecksumText, string StatusLabel)
{
    public static PendingExceptionQuotaView ToPendingExceptionQuotaView(PendingExceptionQuota item)
    {
        return new PendingExceptionQuotaView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Pinned}",
            $"{item.Active}",
            item.Location,
            item.Locale,
            Text.Truncate(item.JobCode, 64),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingExceptionQuotaView> ToPendingExceptionQuotaViews(List<PendingExceptionQuota> items)
    {
        var views = new List<PendingExceptionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPendingExceptionQuotaView(item));
        }
        return views;
    }

    public static double SumPendingExceptionQuotaView(PendingExceptionQuotaView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
