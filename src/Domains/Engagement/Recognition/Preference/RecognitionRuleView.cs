namespace Acme.Workforce.Engagement.Recognition.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition rule.</summary>
public sealed record RecognitionRuleView(string CodeText, string Title, string BatchSize, string ChannelText, string EndsAt, string TenureMonths, string SalaryBandText, string Badge, string StartsAt, string StatusLabel)
{
    public static RecognitionRuleView ToRecognitionRuleView(RecognitionRule item)
    {
        return new RecognitionRuleView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Badge,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionRuleView> ToRecognitionRuleViews(List<RecognitionRule> items)
    {
        var views = new List<RecognitionRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionRuleView(item));
        }
        return views;
    }

    public static double SumRecognitionRuleView(RecognitionRuleView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
