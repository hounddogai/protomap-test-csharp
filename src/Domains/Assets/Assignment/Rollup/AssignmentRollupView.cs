namespace Acme.Workforce.Assets.Assignment.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment rollup.</summary>
public sealed record AssignmentRollupView(string Code, string TitleText, string Reviewer, string Rate, string AmountText, string Minimum, string Threshold, string StatusLabel)
{
    public static AssignmentRollupView ToAssignmentRollupView(AssignmentRollup item)
    {
        return new AssignmentRollupView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Reviewer,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentRollupView> ToAssignmentRollupViews(List<AssignmentRollup> items)
    {
        var views = new List<AssignmentRollupView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentRollupView(item));
        }
        return views;
    }

    public static double SumAssignmentRollupView(AssignmentRollupView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedRate + parsedAmount + parsedMinimum;
    }
}
