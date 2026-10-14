namespace Acme.Workforce.Absence.Holiday.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday rule.</summary>
public sealed record HolidayRuleView(string Code, string Title, string SalaryBand, string Maximum, string GradeText, string Currency, string Timezone, string StatusLabel)
{
    public static HolidayRuleView ToHolidayRuleView(HolidayRule item)
    {
        return new HolidayRuleView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.SalaryBand, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Currency,
            Text.Truncate(item.Timezone, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayRuleView> ToHolidayRuleViews(List<HolidayRule> items)
    {
        var views = new List<HolidayRuleView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayRuleView(item));
        }
        return views;
    }

    public static double SumHolidayRuleView(HolidayRuleView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
