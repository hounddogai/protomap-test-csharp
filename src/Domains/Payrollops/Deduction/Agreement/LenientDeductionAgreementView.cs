namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient deduction agreement.</summary>
public sealed record LenientDeductionAgreementView(string Code, string Title, string EmployeeId, string Visible, string Amount, string Locale, string Region, string SegmentText, string ScoreText, string StatusLabel)
{
    public static LenientDeductionAgreementView ToLenientDeductionAgreementView(LenientDeductionAgreement item)
    {
        return new LenientDeductionAgreementView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.EmployeeId,
            $"{item.Visible}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Region, 80),
            item.Segment,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeductionAgreementView> ToLenientDeductionAgreementViews(List<LenientDeductionAgreement> items)
    {
        var views = new List<LenientDeductionAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeductionAgreementView(item));
        }
        return views;
    }

    public static double SumLenientDeductionAgreementView(LenientDeductionAgreementView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedScore;
    }
}
