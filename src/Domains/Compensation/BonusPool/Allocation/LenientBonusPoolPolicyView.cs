namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient bonus pool policy.</summary>
public sealed record LenientBonusPoolPolicyView(string CodeText, string NameText, string Version, string Hours, string Limit, string FteText, string WindowDays, string ChecksumText, string LocationText, string ExternalRef, string Grade, string StatusLabel)
{
    public static LenientBonusPoolPolicyView ToLenientBonusPoolPolicyView(LenientBonusPoolPolicy item)
    {
        return new LenientBonusPoolPolicyView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Location, 40),
            item.ExternalRef,
            Text.Truncate(item.Grade, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBonusPoolPolicyView> ToLenientBonusPoolPolicyViews(List<LenientBonusPoolPolicy> items)
    {
        var views = new List<LenientBonusPoolPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBonusPoolPolicyView(item));
        }
        return views;
    }

    public static double SumLenientBonusPoolPolicyView(LenientBonusPoolPolicyView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedHours + (double)parsedLimit;
    }
}
