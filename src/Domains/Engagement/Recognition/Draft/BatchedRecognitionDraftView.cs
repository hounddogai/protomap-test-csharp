namespace Acme.Workforce.Engagement.Recognition.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched recognition draft.</summary>
public sealed record BatchedRecognitionDraftView(string Code, string Name, string FloorValue, string SkillText, string Budget, string Phone, string Checksum, string Location, string Weight, string BadgeText, string StatusLabel)
{
    public static BatchedRecognitionDraftView ToBatchedRecognitionDraftView(BatchedRecognitionDraft item)
    {
        return new BatchedRecognitionDraftView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Checksum,
            Text.Truncate(item.Location, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRecognitionDraftView> ToBatchedRecognitionDraftViews(List<BatchedRecognitionDraft> items)
    {
        var views = new List<BatchedRecognitionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRecognitionDraftView(item));
        }
        return views;
    }

    public static double SumBatchedRecognitionDraftView(BatchedRecognitionDraftView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedBudget + parsedWeight;
    }
}
