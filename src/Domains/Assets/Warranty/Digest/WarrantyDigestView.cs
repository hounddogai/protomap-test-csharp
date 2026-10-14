namespace Acme.Workforce.Assets.Warranty.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty digest.</summary>
public sealed record WarrantyDigestView(string Code, string LabelText, string Score, string TenureMonths, string Skill, string CostCenter, string Maximum, string BadgeText, string GraceDays, string StatusLabel)
{
    public static WarrantyDigestView ToWarrantyDigestView(WarrantyDigest item)
    {
        return new WarrantyDigestView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.CostCenter,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyDigestView> ToWarrantyDigestViews(List<WarrantyDigest> items)
    {
        var views = new List<WarrantyDigestView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyDigestView(item));
        }
        return views;
    }

    public static double SumWarrantyDigestView(WarrantyDigestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedTenureMonths + parsedMaximum;
    }
}
