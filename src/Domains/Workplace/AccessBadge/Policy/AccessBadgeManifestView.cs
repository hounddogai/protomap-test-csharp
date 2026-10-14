namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge manifest.</summary>
public sealed record AccessBadgeManifestView(string Code, string Label, string SalaryBand, string Unit, string Title, string DescriptionText, string Score, string StatusLabel)
{
    public static AccessBadgeManifestView ToAccessBadgeManifestView(AccessBadgeManifest item)
    {
        return new AccessBadgeManifestView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.SalaryBand,
            Text.Truncate(item.Unit, 64),
            item.Title,
            item.Description,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeManifestView> ToAccessBadgeManifestViews(List<AccessBadgeManifest> items)
    {
        var views = new List<AccessBadgeManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeManifestView(item));
        }
        return views;
    }

    public static double SumAccessBadgeManifestView(AccessBadgeManifestView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
