namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction entry.</summary>
public sealed record DeductionEntryView(string Code, string Label, string Channel, string Country, string PhoneText, string ScoreText, string StatusLabel)
{
    public static DeductionEntryView ToDeductionEntryView(DeductionEntry item)
    {
        return new DeductionEntryView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Channel,
            item.Country,
            Text.Truncate(item.Phone, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionEntryView> ToDeductionEntryViews(List<DeductionEntry> items)
    {
        var views = new List<DeductionEntryView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionEntryView(item));
        }
        return views;
    }

    public static double SumDeductionEntryView(DeductionEntryView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
