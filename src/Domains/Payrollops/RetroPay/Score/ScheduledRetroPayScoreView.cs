namespace Acme.Workforce.Payrollops.RetroPay.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled retro pay score.</summary>
public sealed record ScheduledRetroPayScoreView(string Code, string Title, string Grade, string ManagerId, string UnitText, string OwnerId, string GraceDays, string Region, string StatusLabel)
{
    public static ScheduledRetroPayScoreView ToScheduledRetroPayScoreView(ScheduledRetroPayScore item)
    {
        return new ScheduledRetroPayScoreView(
            item.Code,
            item.Title,
            Text.Truncate(item.Grade, 40),
            item.ManagerId,
            Text.Truncate(item.Unit, 80),
            item.OwnerId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRetroPayScoreView> ToScheduledRetroPayScoreViews(List<ScheduledRetroPayScore> items)
    {
        var views = new List<ScheduledRetroPayScoreView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRetroPayScoreView(item));
        }
        return views;
    }

    public static double SumScheduledRetroPayScoreView(ScheduledRetroPayScoreView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
