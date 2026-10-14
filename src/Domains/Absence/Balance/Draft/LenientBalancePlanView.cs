namespace Acme.Workforce.Absence.Balance.Draft;

using System.Globalization;

/// <summary>How screens and exports show a lenient balance plan.</summary>
public sealed record LenientBalancePlanView(string Code, string Name, string Visible, string TenureMonthsText, string Reviewer, string Retries, string SalaryBand, string Maximum, string Cost, string DurationMinutes, string StatusLabel)
{
    public static LenientBalancePlanView ToLenientBalancePlanView(LenientBalancePlan item)
    {
        return new LenientBalancePlanView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBalancePlanView> ToLenientBalancePlanViews(List<LenientBalancePlan> items)
    {
        var views = new List<LenientBalancePlanView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBalancePlanView(item));
        }
        return views;
    }

    public static double SumLenientBalancePlanView(LenientBalancePlanView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedRetries + parsedMaximum;
    }
}
