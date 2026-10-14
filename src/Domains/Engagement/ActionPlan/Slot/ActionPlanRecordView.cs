namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan record.</summary>
public sealed record ActionPlanRecordView(string Code, string NameText, string Label, string Attempts, string EndsAt, string BatchSize, string Fte, string StatusLabel)
{
    public static ActionPlanRecordView ToActionPlanRecordView(ActionPlanRecord item)
    {
        return new ActionPlanRecordView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Label, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanRecordView> ToActionPlanRecordViews(List<ActionPlanRecord> items)
    {
        var views = new List<ActionPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanRecordView(item));
        }
        return views;
    }

    public static double SumActionPlanRecordView(ActionPlanRecordView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
