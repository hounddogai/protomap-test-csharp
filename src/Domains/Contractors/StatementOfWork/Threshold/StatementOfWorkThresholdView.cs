namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a statement of work threshold.</summary>
public sealed record StatementOfWorkThresholdView(string Code, string Label, string GraceDays, string Discount, string StartsAt, string Grade, string Rate, string StatusLabel)
{
    public static StatementOfWorkThresholdView ToStatementOfWorkThresholdView(StatementOfWorkThreshold item)
    {
        return new StatementOfWorkThresholdView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkThresholdView> ToStatementOfWorkThresholdViews(List<StatementOfWorkThreshold> items)
    {
        var views = new List<StatementOfWorkThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkThresholdView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkThresholdView(StatementOfWorkThresholdView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedDiscount + (double)parsedStartsAt;
    }
}
