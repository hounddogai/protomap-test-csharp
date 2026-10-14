namespace Acme.Workforce.Orgdesign.Position.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position ledger.</summary>
public sealed record LegacyPositionLedgerView(string Code, string Title, string Ratio, string CeilingText, string Badge, string Checksum, string WindowDays, string TenureMonthsText, string DepartmentText, string SourceText, string SegmentText, string StatusLabel)
{
    public static LegacyPositionLedgerView ToLegacyPositionLedgerView(LegacyPositionLedger item)
    {
        return new LegacyPositionLedgerView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Checksum,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Source,
            Text.Truncate(item.Segment, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionLedgerView> ToLegacyPositionLedgerViews(List<LegacyPositionLedger> items)
    {
        var views = new List<LegacyPositionLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionLedgerView(item));
        }
        return views;
    }

    public static double SumLegacyPositionLedgerView(LegacyPositionLedgerView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCeiling + (double)parsedWindowDays;
    }
}
