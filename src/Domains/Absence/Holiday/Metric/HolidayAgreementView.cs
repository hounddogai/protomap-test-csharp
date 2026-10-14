namespace Acme.Workforce.Absence.Holiday.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday agreement.</summary>
public sealed record HolidayAgreementView(string Code, string NameText, string Cost, string EndsAt, string ContactEmail, string Ratio, string ExpiresAt, string Threshold, string StatusLabel)
{
    public static HolidayAgreementView ToHolidayAgreementView(HolidayAgreement item)
    {
        return new HolidayAgreementView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayAgreementView> ToHolidayAgreementViews(List<HolidayAgreement> items)
    {
        var views = new List<HolidayAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayAgreementView(item));
        }
        return views;
    }

    public static double SumHolidayAgreementView(HolidayAgreementView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedEndsAt + parsedRatio;
    }
}
