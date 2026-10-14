namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped access badge mapping.</summary>
public sealed record CappedAccessBadgeMappingView(string Code, string Name, string Checksum, string Fte, string Attempts, string Reviewer, string SalaryBand, string Label, string Score, string PhoneText, string StatusLabel)
{
    public static CappedAccessBadgeMappingView ToCappedAccessBadgeMappingView(CappedAccessBadgeMapping item)
    {
        return new CappedAccessBadgeMappingView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Checksum, 120),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            item.SalaryBand,
            Text.Truncate(item.Label, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAccessBadgeMappingView> ToCappedAccessBadgeMappingViews(List<CappedAccessBadgeMapping> items)
    {
        var views = new List<CappedAccessBadgeMappingView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAccessBadgeMappingView(item));
        }
        return views;
    }

    public static double SumCappedAccessBadgeMappingView(CappedAccessBadgeMappingView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedAttempts + parsedScore;
    }
}
