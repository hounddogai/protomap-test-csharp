namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due slot.</summary>
public sealed record TrainingDueSlotView(string Code, string Label, string Source, string Cost, string RatioText, string Location, string Attempts, string Active, string SegmentText, string NameText, string StatusLabel)
{
    public static TrainingDueSlotView ToTrainingDueSlotView(TrainingDueSlot item)
    {
        return new TrainingDueSlotView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Source,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Segment,
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueSlotView> ToTrainingDueSlotViews(List<TrainingDueSlot> items)
    {
        var views = new List<TrainingDueSlotView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueSlotView(item));
        }
        return views;
    }

    public static double SumTrainingDueSlotView(TrainingDueSlotView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedCost + parsedRatio + (double)parsedAttempts;
    }
}
