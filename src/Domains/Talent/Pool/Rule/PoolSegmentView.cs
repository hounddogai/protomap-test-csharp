namespace Acme.Workforce.Talent.Pool.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool segment.</summary>
public sealed record PoolSegmentView(string Code, string TitleText, string SalaryBandText, string Fte, string WindowDays, string ManagerId, string StatusLabel)
{
    public static PoolSegmentView ToPoolSegmentView(PoolSegment item)
    {
        return new PoolSegmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.SalaryBand, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolSegmentView> ToPoolSegmentViews(List<PoolSegment> items)
    {
        var views = new List<PoolSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPoolSegmentView(item));
        }
        return views;
    }

    public static double SumPoolSegmentView(PoolSegmentView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedWindowDays;
    }
}
