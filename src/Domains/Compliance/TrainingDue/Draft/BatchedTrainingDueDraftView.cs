namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched training due draft.</summary>
public sealed record BatchedTrainingDueDraftView(string Code, string Title, string Ratio, string FloorValue, string Skill, string Badge, string Location, string ChecksumText, string Department, string StatusLabel)
{
    public static BatchedTrainingDueDraftView ToBatchedTrainingDueDraftView(BatchedTrainingDueDraft item)
    {
        return new BatchedTrainingDueDraftView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Badge, 64),
            Text.Truncate(item.Location, 40),
            item.Checksum,
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTrainingDueDraftView> ToBatchedTrainingDueDraftViews(List<BatchedTrainingDueDraft> items)
    {
        var views = new List<BatchedTrainingDueDraftView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTrainingDueDraftView(item));
        }
        return views;
    }

    public static double SumBatchedTrainingDueDraftView(BatchedTrainingDueDraftView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedRatio + parsedFloorValue;
    }
}
