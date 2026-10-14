namespace Acme.Workforce.Talent.Pool.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pool schedule.</summary>
public sealed record StrictPoolScheduleView(string Code, string Name, string Sequence, string Source, string OwnerId, string ApprovedText, string StatusLabel)
{
    public static StrictPoolScheduleView ToStrictPoolScheduleView(StrictPoolSchedule item)
    {
        return new StrictPoolScheduleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 120),
            item.OwnerId,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPoolScheduleView> ToStrictPoolScheduleViews(List<StrictPoolSchedule> items)
    {
        var views = new List<StrictPoolScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPoolScheduleView(item));
        }
        return views;
    }

    public static double SumStrictPoolScheduleView(StrictPoolScheduleView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
