namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan record.</summary>
public sealed record SuccessionPlanRecordView(string CodeText, string Name, string StartsAtText, string ChannelText, string CreatedAt, string Rating, string Skill, string BatchSize, string GraceDays, string EmployeeId, string StatusLabel)
{
    public static SuccessionPlanRecordView ToSuccessionPlanRecordView(SuccessionPlanRecord item)
    {
        return new SuccessionPlanRecordView(
            item.Code,
            item.Name,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanRecordView> ToSuccessionPlanRecordViews(List<SuccessionPlanRecord> items)
    {
        var views = new List<SuccessionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanRecordView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanRecordView(SuccessionPlanRecordView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedCreatedAt + parsedRating;
    }
}
