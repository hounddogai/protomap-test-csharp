namespace Acme.Workforce.Payrollops.Payslip.Group;

using System.Globalization;

/// <summary>How screens and exports show a payslip window.</summary>
public sealed record PayslipWindowView(string CodeText, string Name, string Rating, string Ceiling, string LocationText, string Summary, string SkillText, string Weight, string Capacity, string Segment, string StatusLabel)
{
    public static PayslipWindowView ToPayslipWindowView(PayslipWindow item)
    {
        return new PayslipWindowView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Summary,
            item.Skill,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipWindowView> ToPayslipWindowViews(List<PayslipWindow> items)
    {
        var views = new List<PayslipWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipWindowView(item));
        }
        return views;
    }

    public static double SumPayslipWindowView(PayslipWindowView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedRating + parsedCeiling + parsedWeight;
    }
}
