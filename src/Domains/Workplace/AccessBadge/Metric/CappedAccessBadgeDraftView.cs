namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped access badge draft.</summary>
public sealed record CappedAccessBadgeDraftView(string Code, string LabelText, string EmployeeIdText, string Ceiling, string Fte, string SummaryText, string LocationText, string StatusLabel)
{
    public static CappedAccessBadgeDraftView ToCappedAccessBadgeDraftView(CappedAccessBadgeDraft item)
    {
        return new CappedAccessBadgeDraftView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 64),
            item.EmployeeId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAccessBadgeDraftView> ToCappedAccessBadgeDraftViews(List<CappedAccessBadgeDraft> items)
    {
        var views = new List<CappedAccessBadgeDraftView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAccessBadgeDraftView(item));
        }
        return views;
    }

    public static double SumCappedAccessBadgeDraftView(CappedAccessBadgeDraftView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedFte;
    }
}
