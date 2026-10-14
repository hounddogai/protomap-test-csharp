namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy rule.</summary>
public sealed record OccupancyRuleView(string Code, string TitleText, string Department, string Revision, string Phone, string GradeText, string Rate, string DiscountText, string SalaryBand, string Skill, string StatusLabel)
{
    public static OccupancyRuleView ToOccupancyRuleView(OccupancyRule item)
    {
        return new OccupancyRuleView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Department,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.Grade,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.Skill, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyRuleView> ToOccupancyRuleViews(List<OccupancyRule> items)
    {
        var views = new List<OccupancyRuleView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyRuleView(item));
        }
        return views;
    }

    public static double SumOccupancyRuleView(OccupancyRuleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRate + parsedDiscount;
    }
}
