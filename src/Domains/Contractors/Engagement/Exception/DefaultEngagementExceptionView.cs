namespace Acme.Workforce.Contractors.Engagement.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default engagement exception.</summary>
public sealed record DefaultEngagementExceptionView(string CodeText, string Name, string CapacityText, string Quantity, string SummaryText, string ManagerId, string RatioText, string Currency, string Rate, string StatusLabel)
{
    public static DefaultEngagementExceptionView ToDefaultEngagementExceptionView(DefaultEngagementException item)
    {
        return new DefaultEngagementExceptionView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.ManagerId, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultEngagementExceptionView> ToDefaultEngagementExceptionViews(List<DefaultEngagementException> items)
    {
        var views = new List<DefaultEngagementExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultEngagementExceptionView(item));
        }
        return views;
    }

    public static double SumDefaultEngagementExceptionView(DefaultEngagementExceptionView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedQuantity + parsedRatio;
    }
}
