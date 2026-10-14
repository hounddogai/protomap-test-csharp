namespace Acme.Workforce.Talent.Pool.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool threshold.</summary>
public sealed record PoolThresholdView(string CodeText, string Name, string Department, string SkillText, string Team, string Region, string Phone, string Segment, string ExpiresAtText, string StatusLabel)
{
    public static PoolThresholdView ToPoolThresholdView(PoolThreshold item)
    {
        return new PoolThresholdView(
            item.Code,
            item.Name,
            item.Department,
            item.Skill,
            item.Team,
            item.Region,
            Text.Truncate(item.Phone, 80),
            item.Segment,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolThresholdView> ToPoolThresholdViews(List<PoolThreshold> items)
    {
        var views = new List<PoolThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPoolThresholdView(item));
        }
        return views;
    }

    public static double SumPoolThresholdView(PoolThresholdView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
