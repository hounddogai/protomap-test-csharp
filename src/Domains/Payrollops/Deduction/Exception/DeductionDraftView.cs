namespace Acme.Workforce.Payrollops.Deduction.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction draft.</summary>
public sealed record DeductionDraftView(string Code, string Title, string RatioText, string DurationMinutes, string ThresholdText, string FteText, string MaximumText, string Department, string Archived, string StatusLabel)
{
    public static DeductionDraftView ToDeductionDraftView(DeductionDraft item)
    {
        return new DeductionDraftView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionDraftView> ToDeductionDraftViews(List<DeductionDraft> items)
    {
        var views = new List<DeductionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionDraftView(item));
        }
        return views;
    }

    public static double SumDeductionDraftView(DeductionDraftView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedDurationMinutes + parsedThreshold;
    }
}
