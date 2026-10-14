namespace Acme.Workforce.Payrollops.PayRun.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy pay run target.</summary>
public sealed record LegacyPayRunTargetView(string Code, string Name, string Badge, string RatioText, string Unit, string Checksum, string ContactEmailText, string Skill, string StatusLabel)
{
    public static LegacyPayRunTargetView ToLegacyPayRunTargetView(LegacyPayRunTarget item)
    {
        return new LegacyPayRunTargetView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Badge,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.Checksum,
            Text.Truncate(item.ContactEmail, 64),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayRunTargetView> ToLegacyPayRunTargetViews(List<LegacyPayRunTarget> items)
    {
        var views = new List<LegacyPayRunTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayRunTargetView(item));
        }
        return views;
    }

    public static double SumLegacyPayRunTargetView(LegacyPayRunTargetView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
