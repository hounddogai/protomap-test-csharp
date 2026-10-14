namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey exception.</summary>
public sealed record PulseSurveyExceptionView(string CodeText, string Title, string Budget, string RetriesText, string Badge, string EndsAt, string ExternalRefText, string WeightText, string Category, string Region, string StatusLabel)
{
    public static PulseSurveyExceptionView ToPulseSurveyExceptionView(PulseSurveyException item)
    {
        return new PulseSurveyExceptionView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyExceptionView> ToPulseSurveyExceptionViews(List<PulseSurveyException> items)
    {
        var views = new List<PulseSurveyExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyExceptionView(item));
        }
        return views;
    }

    public static double SumPulseSurveyExceptionView(PulseSurveyExceptionView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedRetries + (double)parsedEndsAt;
    }
}
