namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted training due audit.</summary>
public sealed record WeightedTrainingDueAuditView(string Code, string Title, string Sequence, string Segment, string CostCenterText, string TimezoneText, string Hours, string Unit, string StatusLabel)
{
    public static WeightedTrainingDueAuditView ToWeightedTrainingDueAuditView(WeightedTrainingDueAudit item)
    {
        return new WeightedTrainingDueAuditView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 40),
            item.CostCenter,
            item.Timezone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedTrainingDueAuditView> ToWeightedTrainingDueAuditViews(List<WeightedTrainingDueAudit> items)
    {
        var views = new List<WeightedTrainingDueAuditView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedTrainingDueAuditView(item));
        }
        return views;
    }

    public static double SumWeightedTrainingDueAuditView(WeightedTrainingDueAuditView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedHours;
    }
}
