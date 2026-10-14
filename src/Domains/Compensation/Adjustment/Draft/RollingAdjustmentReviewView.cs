namespace Acme.Workforce.Compensation.Adjustment.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling adjustment review.</summary>
public sealed record RollingAdjustmentReviewView(string CodeText, string LabelText, string Team, string ExternalRefText, string ManagerId, string SalaryBand, string Rate, string LeaveDays, string Timezone, string JobCode, string Channel, string StatusLabel)
{
    public static RollingAdjustmentReviewView ToRollingAdjustmentReviewView(RollingAdjustmentReview item)
    {
        return new RollingAdjustmentReviewView(
            item.Code,
            item.Label,
            item.Team,
            item.ExternalRef,
            Text.Truncate(item.ManagerId, 80),
            Text.Truncate(item.SalaryBand, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.JobCode, 120),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingAdjustmentReviewView> ToRollingAdjustmentReviewViews(List<RollingAdjustmentReview> items)
    {
        var views = new List<RollingAdjustmentReviewView>();
        foreach (var item in items)
        {
            views.Add(ToRollingAdjustmentReviewView(item));
        }
        return views;
    }

    public static double SumRollingAdjustmentReviewView(RollingAdjustmentReviewView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedLeaveDays;
    }
}
