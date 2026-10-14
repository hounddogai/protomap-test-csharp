namespace Acme.Workforce.Payrollops.Payslip.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback payslip template.</summary>
public sealed record FallbackPayslipTemplateView(string Code, string Name, string FloorValueText, string Sequence, string UnitText, string EndsAtText, string Rate, string StatusLabel)
{
    public static FallbackPayslipTemplateView ToFallbackPayslipTemplateView(FallbackPayslipTemplate item)
    {
        return new FallbackPayslipTemplateView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayslipTemplateView> ToFallbackPayslipTemplateViews(List<FallbackPayslipTemplate> items)
    {
        var views = new List<FallbackPayslipTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayslipTemplateView(item));
        }
        return views;
    }

    public static double SumFallbackPayslipTemplateView(FallbackPayslipTemplateView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedSequence + (double)parsedEndsAt;
    }
}
