namespace Acme.Workforce.Payrollops.Garnishment.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment score.</summary>
public sealed record GarnishmentScoreView(string Code, string Name, string Archived, string EmployeeIdText, string DurationMinutes, string Location, string Phone, string UpdatedAtText, string Sequence, string StatusLabel)
{
    public static GarnishmentScoreView ToGarnishmentScoreView(GarnishmentScore item)
    {
        return new GarnishmentScoreView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Archived}",
            item.EmployeeId,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            item.Phone,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentScoreView> ToGarnishmentScoreViews(List<GarnishmentScore> items)
    {
        var views = new List<GarnishmentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentScoreView(item));
        }
        return views;
    }

    public static double SumGarnishmentScoreView(GarnishmentScoreView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedUpdatedAt + (double)parsedSequence;
    }
}
