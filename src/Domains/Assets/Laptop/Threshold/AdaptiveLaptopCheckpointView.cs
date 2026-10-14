namespace Acme.Workforce.Assets.Laptop.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a adaptive laptop checkpoint.</summary>
public sealed record AdaptiveLaptopCheckpointView(string CodeText, string LabelText, string Budget, string RetriesText, string DepartmentText, string Cost, string Ceiling, string ManagerId, string OwnerId, string Ratio, string StatusLabel)
{
    public static AdaptiveLaptopCheckpointView ToAdaptiveLaptopCheckpointView(AdaptiveLaptopCheckpoint item)
    {
        return new AdaptiveLaptopCheckpointView(
            item.Code,
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.OwnerId,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveLaptopCheckpointView> ToAdaptiveLaptopCheckpointViews(List<AdaptiveLaptopCheckpoint> items)
    {
        var views = new List<AdaptiveLaptopCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveLaptopCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveLaptopCheckpointView(AdaptiveLaptopCheckpointView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedRetries + parsedCost;
    }
}
