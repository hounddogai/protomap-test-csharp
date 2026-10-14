namespace Acme.Workforce.Absence.Policy.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary policy item.</summary>
public sealed record PrimaryPolicyItemView(string Code, string Name, string Weight, string Badge, string DepartmentText, string Segment, string ScoreText, string FloorValue, string StatusLabel)
{
    public static PrimaryPolicyItemView ToPrimaryPolicyItemView(PrimaryPolicyItem item)
    {
        return new PrimaryPolicyItemView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Department,
            Text.Truncate(item.Segment, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPolicyItemView> ToPrimaryPolicyItemViews(List<PrimaryPolicyItem> items)
    {
        var views = new List<PrimaryPolicyItemView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPolicyItemView(item));
        }
        return views;
    }

    public static double SumPrimaryPolicyItemView(PrimaryPolicyItemView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedWeight + parsedScore + parsedFloorValue;
    }
}
