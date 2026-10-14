namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered career path override.</summary>
public sealed record TieredCareerPathOverrideView(string Code, string Label, string JobCodeText, string ExpiresAt, string Location, string ActiveText, string PinnedText, string StatusLabel)
{
    public static TieredCareerPathOverrideView ToTieredCareerPathOverrideView(TieredCareerPathOverride item)
    {
        return new TieredCareerPathOverrideView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.JobCode, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            $"{item.Active}",
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCareerPathOverrideView> ToTieredCareerPathOverrideViews(List<TieredCareerPathOverride> items)
    {
        var views = new List<TieredCareerPathOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCareerPathOverrideView(item));
        }
        return views;
    }

    public static double SumTieredCareerPathOverrideView(TieredCareerPathOverrideView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
