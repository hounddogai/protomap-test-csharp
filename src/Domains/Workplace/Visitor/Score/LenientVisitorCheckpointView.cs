namespace Acme.Workforce.Workplace.Visitor.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient visitor checkpoint.</summary>
public sealed record LenientVisitorCheckpointView(string Code, string Name, string FloorValue, string Archived, string LabelText, string EndsAt, string TenureMonthsText, string StatusLabel)
{
    public static LenientVisitorCheckpointView ToLenientVisitorCheckpointView(LenientVisitorCheckpoint item)
    {
        return new LenientVisitorCheckpointView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Label, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientVisitorCheckpointView> ToLenientVisitorCheckpointViews(List<LenientVisitorCheckpoint> items)
    {
        var views = new List<LenientVisitorCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLenientVisitorCheckpointView(item));
        }
        return views;
    }

    public static double SumLenientVisitorCheckpointView(LenientVisitorCheckpointView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
