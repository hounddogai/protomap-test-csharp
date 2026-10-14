namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered training due variance.</summary>
public sealed record TieredTrainingDueVarianceView(string Code, string Title, string QuantityText, string DescriptionText, string Fte, string DurationMinutesText, string TenureMonths, string Budget, string Segment, string StatusLabel)
{
    public static TieredTrainingDueVarianceView ToTieredTrainingDueVarianceView(TieredTrainingDueVariance item)
    {
        return new TieredTrainingDueVarianceView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredTrainingDueVarianceView> ToTieredTrainingDueVarianceViews(List<TieredTrainingDueVariance> items)
    {
        var views = new List<TieredTrainingDueVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToTieredTrainingDueVarianceView(item));
        }
        return views;
    }

    public static double SumTieredTrainingDueVarianceView(TieredTrainingDueVarianceView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedFte + (double)parsedDurationMinutes;
    }
}
