namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual pulse survey digest.</summary>
public sealed record ManualPulseSurveyDigestView(string CodeText, string Name, string VisibleText, string Approved, string Title, string Summary, string GraceDays, string BudgetText, string Limit, string StatusLabel)
{
    public static ManualPulseSurveyDigestView ToManualPulseSurveyDigestView(ManualPulseSurveyDigest item)
    {
        return new ManualPulseSurveyDigestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            $"{item.Visible}",
            $"{item.Approved}",
            item.Title,
            item.Summary,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPulseSurveyDigestView> ToManualPulseSurveyDigestViews(List<ManualPulseSurveyDigest> items)
    {
        var views = new List<ManualPulseSurveyDigestView>();
        foreach (var item in items)
        {
            views.Add(ToManualPulseSurveyDigestView(item));
        }
        return views;
    }

    public static double SumManualPulseSurveyDigestView(ManualPulseSurveyDigestView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedBudget + (double)parsedLimit;
    }
}
