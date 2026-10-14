namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched license seat profile.</summary>
public sealed record BatchedLicenseSeatProfileView(string CodeText, string NameText, string CapacityText, string CreatedAt, string BudgetText, string JobCode, string BadgeText, string FloorValueText, string StatusLabel)
{
    public static BatchedLicenseSeatProfileView ToBatchedLicenseSeatProfileView(BatchedLicenseSeatProfile item)
    {
        return new BatchedLicenseSeatProfileView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Badge,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedLicenseSeatProfileView> ToBatchedLicenseSeatProfileViews(List<BatchedLicenseSeatProfile> items)
    {
        var views = new List<BatchedLicenseSeatProfileView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedLicenseSeatProfileView(item));
        }
        return views;
    }

    public static double SumBatchedLicenseSeatProfileView(BatchedLicenseSeatProfileView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedCreatedAt + parsedBudget;
    }
}
