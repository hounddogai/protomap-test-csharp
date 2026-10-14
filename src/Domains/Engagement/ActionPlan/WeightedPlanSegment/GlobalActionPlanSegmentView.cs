namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using System.Globalization;

/// <summary>How screens and exports show a global action plan segment.</summary>
public sealed record GlobalActionPlanSegmentView(string CodeText, string Name, string AttemptsText, string Ratio, string ManagerId, string StatusLabel)
{
    public static GlobalActionPlanSegmentView ToGlobalActionPlanSegmentView(GlobalActionPlanSegment item)
    {
        return new GlobalActionPlanSegmentView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalActionPlanSegmentView> ToGlobalActionPlanSegmentViews(List<GlobalActionPlanSegment> items)
    {
        var views = new List<GlobalActionPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalActionPlanSegmentView(item));
        }
        return views;
    }

    public static double SumGlobalActionPlanSegmentView(GlobalActionPlanSegmentView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRatio;
    }
}
