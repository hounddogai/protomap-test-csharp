namespace Acme.Workforce.Absence.Accrual.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual target.</summary>
public sealed record AccrualTargetView(string CodeText, string Name, string Region, string ChannelText, string QuantityText, string Threshold, string Source, string CreatedAt, string ManagerId, string Retries, string GraceDays, string StatusLabel)
{
    public static AccrualTargetView ToAccrualTargetView(AccrualTarget item)
    {
        return new AccrualTargetView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Region, 40),
            Text.Truncate(item.Channel, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualTargetView> ToAccrualTargetViews(List<AccrualTarget> items)
    {
        var views = new List<AccrualTargetView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualTargetView(item));
        }
        return views;
    }

    public static double SumAccrualTargetView(AccrualTargetView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedThreshold + (double)parsedCreatedAt;
    }
}
