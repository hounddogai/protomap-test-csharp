namespace Acme.Workforce.Engagement.Feedback.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending feedback mapping.</summary>
public sealed record PendingFeedbackMappingView(string Code, string Name, string MaximumText, string TitleText, string EmployeeIdText, string ExpiresAt, string Limit, string ContactEmail, string StatusLabel)
{
    public static PendingFeedbackMappingView ToPendingFeedbackMappingView(PendingFeedbackMapping item)
    {
        return new PendingFeedbackMappingView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.EmployeeId, 64),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingFeedbackMappingView> ToPendingFeedbackMappingViews(List<PendingFeedbackMapping> items)
    {
        var views = new List<PendingFeedbackMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPendingFeedbackMappingView(item));
        }
        return views;
    }

    public static double SumPendingFeedbackMappingView(PendingFeedbackMappingView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedExpiresAt + (double)parsedLimit;
    }
}
