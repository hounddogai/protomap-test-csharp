namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan segment.</summary>
public sealed record ReturnPlanSegmentView(string Code, string NameText, string GraceDays, string StartsAt, string Segment, string ScoreText, string Fte, string ContactEmail, string Ratio, string Summary, string Discount, string StatusLabel)
{
    public static ReturnPlanSegmentView ToReturnPlanSegmentView(ReturnPlanSegment item)
    {
        return new ReturnPlanSegmentView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanSegmentView> ToReturnPlanSegmentViews(List<ReturnPlanSegment> items)
    {
        var views = new List<ReturnPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanSegmentView(item));
        }
        return views;
    }

    public static double SumReturnPlanSegmentView(ReturnPlanSegmentView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedStartsAt + parsedScore;
    }
}
