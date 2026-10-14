namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled statement of work variance.</summary>
public sealed record ScheduledStatementOfWorkVarianceView(string Code, string Name, string Grade, string StartsAt, string WeightText, string RegionText, string TimezoneText, string UpdatedAt, string SegmentText, string StatusLabel)
{
    public static ScheduledStatementOfWorkVarianceView ToScheduledStatementOfWorkVarianceView(ScheduledStatementOfWorkVariance item)
    {
        return new ScheduledStatementOfWorkVarianceView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Grade,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            Text.Truncate(item.Timezone, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledStatementOfWorkVarianceView> ToScheduledStatementOfWorkVarianceViews(List<ScheduledStatementOfWorkVariance> items)
    {
        var views = new List<ScheduledStatementOfWorkVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledStatementOfWorkVarianceView(item));
        }
        return views;
    }

    public static double SumScheduledStatementOfWorkVarianceView(ScheduledStatementOfWorkVarianceView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedWeight + (double)parsedUpdatedAt;
    }
}
