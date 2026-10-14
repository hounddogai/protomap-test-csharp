namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient bonus pool digest.</summary>
public sealed record LenientBonusPoolDigestView(string CodeText, string Title, string DurationMinutesText, string Segment, string TenureMonths, string StatusLabel)
{
    public static LenientBonusPoolDigestView ToLenientBonusPoolDigestView(LenientBonusPoolDigest item)
    {
        return new LenientBonusPoolDigestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBonusPoolDigestView> ToLenientBonusPoolDigestViews(List<LenientBonusPoolDigest> items)
    {
        var views = new List<LenientBonusPoolDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBonusPoolDigestView(item));
        }
        return views;
    }

    public static double SumLenientBonusPoolDigestView(LenientBonusPoolDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedTenureMonths;
    }
}
