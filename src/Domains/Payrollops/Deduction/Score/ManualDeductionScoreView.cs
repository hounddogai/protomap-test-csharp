namespace Acme.Workforce.Payrollops.Deduction.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual deduction score.</summary>
public sealed record ManualDeductionScoreView(string CodeText, string LabelText, string StartsAt, string PhoneText, string Summary, string GraceDays, string StatusLabel)
{
    public static ManualDeductionScoreView ToManualDeductionScoreView(ManualDeductionScore item)
    {
        return new ManualDeductionScoreView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 80),
            item.Summary,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualDeductionScoreView> ToManualDeductionScoreViews(List<ManualDeductionScore> items)
    {
        var views = new List<ManualDeductionScoreView>();
        foreach (var item in items)
        {
            views.Add(ToManualDeductionScoreView(item));
        }
        return views;
    }

    public static double SumManualDeductionScoreView(ManualDeductionScoreView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedGraceDays;
    }
}
