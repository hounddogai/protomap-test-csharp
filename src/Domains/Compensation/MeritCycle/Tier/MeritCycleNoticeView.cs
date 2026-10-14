namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle notice.</summary>
public sealed record MeritCycleNoticeView(string Code, string Label, string Title, string SalaryBand, string FloorValue, string Maximum, string StatusLabel)
{
    public static MeritCycleNoticeView ToMeritCycleNoticeView(MeritCycleNotice item)
    {
        return new MeritCycleNoticeView(
            item.Code,
            item.Label,
            item.Title,
            Text.Truncate(item.SalaryBand, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleNoticeView> ToMeritCycleNoticeViews(List<MeritCycleNotice> items)
    {
        var views = new List<MeritCycleNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleNoticeView(item));
        }
        return views;
    }

    public static double SumMeritCycleNoticeView(MeritCycleNoticeView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMaximum;
    }
}
