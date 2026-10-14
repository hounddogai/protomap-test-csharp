namespace Acme.Workforce.Absence.Accrual.Manifest;

using System.Globalization;

/// <summary>How screens and exports show a accrual plan.</summary>
public sealed record AccrualPlanView(string CodeText, string Title, string Rating, string JobCode, string QuantityText, string Priority, string StatusLabel)
{
    public static AccrualPlanView ToAccrualPlanView(AccrualPlan item)
    {
        return new AccrualPlanView(
            item.Code,
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualPlanView> ToAccrualPlanViews(List<AccrualPlan> items)
    {
        var views = new List<AccrualPlanView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualPlanView(item));
        }
        return views;
    }

    public static double SumAccrualPlanView(AccrualPlanView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedQuantity + (double)parsedPriority;
    }
}
