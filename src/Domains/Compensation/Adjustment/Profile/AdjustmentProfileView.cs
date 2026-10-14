namespace Acme.Workforce.Compensation.Adjustment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment profile.</summary>
public sealed record AdjustmentProfileView(string Code, string NameText, string Team, string Score, string GraceDays, string LimitText, string LabelText, string Rate, string WindowDaysText, string EmployeeId, string StatusLabel)
{
    public static AdjustmentProfileView ToAdjustmentProfileView(AdjustmentProfile item)
    {
        return new AdjustmentProfileView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Team,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentProfileView> ToAdjustmentProfileViews(List<AdjustmentProfile> items)
    {
        var views = new List<AdjustmentProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentProfileView(item));
        }
        return views;
    }

    public static double SumAdjustmentProfileView(AdjustmentProfileView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedGraceDays + (double)parsedLimit;
    }
}
