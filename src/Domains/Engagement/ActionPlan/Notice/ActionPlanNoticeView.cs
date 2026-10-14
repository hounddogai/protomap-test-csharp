namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using System.Globalization;

/// <summary>How screens and exports show a action plan notice.</summary>
public sealed record ActionPlanNoticeView(string CodeText, string Name, string DurationMinutesText, string Badge, string SequenceText, string FloorValue, string StatusLabel)
{
    public static ActionPlanNoticeView ToActionPlanNoticeView(ActionPlanNotice item)
    {
        return new ActionPlanNoticeView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanNoticeView> ToActionPlanNoticeViews(List<ActionPlanNotice> items)
    {
        var views = new List<ActionPlanNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanNoticeView(item));
        }
        return views;
    }

    public static double SumActionPlanNoticeView(ActionPlanNoticeView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedSequence + parsedFloorValue;
    }
}
