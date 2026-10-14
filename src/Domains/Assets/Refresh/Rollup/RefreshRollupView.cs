namespace Acme.Workforce.Assets.Refresh.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh rollup.</summary>
public sealed record RefreshRollupView(string Code, string LabelText, string Skill, string Sequence, string Rate, string EmployeeIdText, string Discount, string Team, string DescriptionText, string StatusLabel)
{
    public static RefreshRollupView ToRefreshRollupView(RefreshRollup item)
    {
        return new RefreshRollupView(
            item.Code,
            item.Label,
            item.Skill,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 64),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshRollupView> ToRefreshRollupViews(List<RefreshRollup> items)
    {
        var views = new List<RefreshRollupView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshRollupView(item));
        }
        return views;
    }

    public static double SumRefreshRollupView(RefreshRollupView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedRate + parsedDiscount;
    }
}
