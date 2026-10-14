namespace Acme.Workforce.Absence.Accrual.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual batch.</summary>
public sealed record AccrualBatchView(string Code, string Name, string Rating, string OwnerId, string Revision, string SalaryBandText, string EmployeeIdText, string GraceDaysText, string Country, string StatusLabel)
{
    public static AccrualBatchView ToAccrualBatchView(AccrualBatch item)
    {
        return new AccrualBatchView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.EmployeeId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualBatchView> ToAccrualBatchViews(List<AccrualBatch> items)
    {
        var views = new List<AccrualBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualBatchView(item));
        }
        return views;
    }

    public static double SumAccrualBatchView(AccrualBatchView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedRevision + (double)parsedGraceDays;
    }
}
