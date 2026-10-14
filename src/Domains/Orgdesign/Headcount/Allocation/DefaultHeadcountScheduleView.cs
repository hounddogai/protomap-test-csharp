namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default headcount schedule.</summary>
public sealed record DefaultHeadcountScheduleView(string CodeText, string Title, string BatchSize, string UpdatedAt, string Visible, string StatusLabel)
{
    public static DefaultHeadcountScheduleView ToDefaultHeadcountScheduleView(DefaultHeadcountSchedule item)
    {
        return new DefaultHeadcountScheduleView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHeadcountScheduleView> ToDefaultHeadcountScheduleViews(List<DefaultHeadcountSchedule> items)
    {
        var views = new List<DefaultHeadcountScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHeadcountScheduleView(item));
        }
        return views;
    }

    public static double SumDefaultHeadcountScheduleView(DefaultHeadcountScheduleView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedUpdatedAt;
    }
}
