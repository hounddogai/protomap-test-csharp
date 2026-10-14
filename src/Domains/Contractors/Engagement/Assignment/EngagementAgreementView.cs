namespace Acme.Workforce.Contractors.Engagement.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement agreement.</summary>
public sealed record EngagementAgreementView(string Code, string Label, string ManagerId, string TitleText, string DurationMinutes, string BatchSize, string StatusLabel)
{
    public static EngagementAgreementView ToEngagementAgreementView(EngagementAgreement item)
    {
        return new EngagementAgreementView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.ManagerId,
            item.Title,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementAgreementView> ToEngagementAgreementViews(List<EngagementAgreement> items)
    {
        var views = new List<EngagementAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementAgreementView(item));
        }
        return views;
    }

    public static double SumEngagementAgreementView(EngagementAgreementView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedBatchSize;
    }
}
