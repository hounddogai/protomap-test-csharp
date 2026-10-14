namespace Acme.Workforce.Assets.Assignment.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped assignment item.</summary>
public sealed record CappedAssignmentItemView(string Code, string Label, string Department, string Region, string CostCenter, string Score, string StatusLabel)
{
    public static CappedAssignmentItemView ToCappedAssignmentItemView(CappedAssignmentItem item)
    {
        return new CappedAssignmentItemView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 120),
            item.Department,
            item.Region,
            Text.Truncate(item.CostCenter, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAssignmentItemView> ToCappedAssignmentItemViews(List<CappedAssignmentItem> items)
    {
        var views = new List<CappedAssignmentItemView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAssignmentItemView(item));
        }
        return views;
    }

    public static double SumCappedAssignmentItemView(CappedAssignmentItemView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
