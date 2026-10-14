namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant schedule.</summary>
public sealed record EquityGrantScheduleView(string Code, string Title, string Phone, string Visible, string WeightText, string Margin, string StatusLabel)
{
    public static EquityGrantScheduleView ToEquityGrantScheduleView(EquityGrantSchedule item)
    {
        return new EquityGrantScheduleView(
            item.Code,
            item.Title,
            Text.Truncate(item.Phone, 80),
            $"{item.Visible}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantScheduleView> ToEquityGrantScheduleViews(List<EquityGrantSchedule> items)
    {
        var views = new List<EquityGrantScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantScheduleView(item));
        }
        return views;
    }

    public static double SumEquityGrantScheduleView(EquityGrantScheduleView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedWeight + parsedMargin;
    }
}
