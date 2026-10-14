namespace Acme.Workforce.Absence.Accrual.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy accrual segment.</summary>
public sealed record LegacyAccrualSegmentView(string Code, string TitleText, string Score, string CostCenterText, string UpdatedAt, string Notes, string StatusLabel)
{
    public static LegacyAccrualSegmentView ToLegacyAccrualSegmentView(LegacyAccrualSegment item)
    {
        return new LegacyAccrualSegmentView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyAccrualSegmentView> ToLegacyAccrualSegmentViews(List<LegacyAccrualSegment> items)
    {
        var views = new List<LegacyAccrualSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyAccrualSegmentView(item));
        }
        return views;
    }

    public static double SumLegacyAccrualSegmentView(LegacyAccrualSegmentView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedUpdatedAt;
    }
}
