namespace Acme.Workforce.Contractors.Rate.Segment;

using System.Globalization;

/// <summary>How screens and exports show a rolling rate variance.</summary>
public sealed record RollingRateVarianceView(string Code, string Title, string UpdatedAtText, string Reviewer, string ApprovedText, string Priority, string Country, string CostCenter, string Maximum, string GraceDays, string StatusLabel)
{
    public static RollingRateVarianceView ToRollingRateVarianceView(RollingRateVariance item)
    {
        return new RollingRateVarianceView(
            item.Code,
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Approved}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.CostCenter,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingRateVarianceView> ToRollingRateVarianceViews(List<RollingRateVariance> items)
    {
        var views = new List<RollingRateVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingRateVarianceView(item));
        }
        return views;
    }

    public static double SumRollingRateVarianceView(RollingRateVarianceView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedPriority + parsedMaximum;
    }
}
