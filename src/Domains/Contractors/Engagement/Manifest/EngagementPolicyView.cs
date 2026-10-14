namespace Acme.Workforce.Contractors.Engagement.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement policy.</summary>
public sealed record EngagementPolicyView(string Code, string Title, string FloorValueText, string Sequence, string UpdatedAt, string Team, string Active, string Approved, string MinimumText, string Score, string StatusLabel)
{
    public static EngagementPolicyView ToEngagementPolicyView(EngagementPolicy item)
    {
        return new EngagementPolicyView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            $"{item.Active}",
            $"{item.Approved}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementPolicyView> ToEngagementPolicyViews(List<EngagementPolicy> items)
    {
        var views = new List<EngagementPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementPolicyView(item));
        }
        return views;
    }

    public static double SumEngagementPolicyView(EngagementPolicyView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedSequence + (double)parsedUpdatedAt;
    }
}
