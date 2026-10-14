namespace Acme.Workforce.Compensation.BonusPool.Batch;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool request.</summary>
public sealed record BonusPoolRequestView(string Code, string Title, string BatchSizeText, string Channel, string Visible, string StartsAt, string SalaryBand, string Cost, string GradeText, string Notes, string DurationMinutes, string StatusLabel)
{
    public static BonusPoolRequestView ToBonusPoolRequestView(BonusPoolRequest item)
    {
        return new BonusPoolRequestView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Visible}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Notes,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolRequestView> ToBonusPoolRequestViews(List<BonusPoolRequest> items)
    {
        var views = new List<BonusPoolRequestView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolRequestView(item));
        }
        return views;
    }

    public static double SumBonusPoolRequestView(BonusPoolRequestView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedStartsAt + parsedCost;
    }
}
