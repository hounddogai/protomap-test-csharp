namespace Acme.Workforce.Payrollops.PayRun.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run quota.</summary>
public sealed record PayRunQuotaView(string CodeText, string NameText, string ExpiresAtText, string Channel, string BatchSize, string Rating, string Checksum, string RevisionText, string StatusLabel)
{
    public static PayRunQuotaView ToPayRunQuotaView(PayRunQuota item)
    {
        return new PayRunQuotaView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunQuotaView> ToPayRunQuotaViews(List<PayRunQuota> items)
    {
        var views = new List<PayRunQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunQuotaView(item));
        }
        return views;
    }

    public static double SumPayRunQuotaView(PayRunQuotaView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedBatchSize + parsedRating;
    }
}
