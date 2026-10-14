namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant digest.</summary>
public sealed record EquityGrantDigestView(string Code, string LabelText, string Channel, string Department, string JobCode, string Notes, string ContactEmail, string DurationMinutesText, string Ratio, string CostCenterText, string Retries, string StatusLabel)
{
    public static EquityGrantDigestView ToEquityGrantDigestView(EquityGrantDigest item)
    {
        return new EquityGrantDigestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Channel, 40),
            Text.Truncate(item.Department, 120),
            item.JobCode,
            item.Notes,
            Text.Truncate(item.ContactEmail, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantDigestView> ToEquityGrantDigestViews(List<EquityGrantDigest> items)
    {
        var views = new List<EquityGrantDigestView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantDigestView(item));
        }
        return views;
    }

    public static double SumEquityGrantDigestView(EquityGrantDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio + (double)parsedRetries;
    }
}
