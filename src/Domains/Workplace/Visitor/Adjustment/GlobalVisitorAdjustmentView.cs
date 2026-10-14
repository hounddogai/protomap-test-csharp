namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global visitor adjustment.</summary>
public sealed record GlobalVisitorAdjustmentView(string Code, string LabelText, string Rate, string OwnerId, string ManagerId, string RetriesText, string Summary, string Grade, string Quantity, string StatusLabel)
{
    public static GlobalVisitorAdjustmentView ToGlobalVisitorAdjustmentView(GlobalVisitorAdjustment item)
    {
        return new GlobalVisitorAdjustmentView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.ManagerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Grade,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalVisitorAdjustmentView> ToGlobalVisitorAdjustmentViews(List<GlobalVisitorAdjustment> items)
    {
        var views = new List<GlobalVisitorAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalVisitorAdjustmentView(item));
        }
        return views;
    }

    public static double SumGlobalVisitorAdjustmentView(GlobalVisitorAdjustmentView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedRetries + (double)parsedQuantity;
    }
}
