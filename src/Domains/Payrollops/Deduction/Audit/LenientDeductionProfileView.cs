namespace Acme.Workforce.Payrollops.Deduction.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient deduction profile.</summary>
public sealed record LenientDeductionProfileView(string Code, string NameText, string Ceiling, string RatioText, string Summary, string Category, string StatusLabel)
{
    public static LenientDeductionProfileView ToLenientDeductionProfileView(LenientDeductionProfile item)
    {
        return new LenientDeductionProfileView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.Category, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeductionProfileView> ToLenientDeductionProfileViews(List<LenientDeductionProfile> items)
    {
        var views = new List<LenientDeductionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeductionProfileView(item));
        }
        return views;
    }

    public static double SumLenientDeductionProfileView(LenientDeductionProfileView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedRatio;
    }
}
