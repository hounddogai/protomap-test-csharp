namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy occupancy draft.</summary>
public sealed record LegacyOccupancyDraftView(string Code, string Title, string Department, string TenureMonths, string Summary, string Sequence, string Unit, string Location, string Weight, string EmployeeIdText, string StatusLabel)
{
    public static LegacyOccupancyDraftView ToLegacyOccupancyDraftView(LegacyOccupancyDraft item)
    {
        return new LegacyOccupancyDraftView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Department,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Location,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyOccupancyDraftView> ToLegacyOccupancyDraftViews(List<LegacyOccupancyDraft> items)
    {
        var views = new List<LegacyOccupancyDraftView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyOccupancyDraftView(item));
        }
        return views;
    }

    public static double SumLegacyOccupancyDraftView(LegacyOccupancyDraftView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedSequence + parsedWeight;
    }
}
