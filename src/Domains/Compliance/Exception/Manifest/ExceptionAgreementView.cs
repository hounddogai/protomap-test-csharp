namespace Acme.Workforce.Compliance.Exception.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception agreement.</summary>
public sealed record ExceptionAgreementView(string Code, string Title, string DurationMinutes, string RetriesText, string Attempts, string StatusLabel)
{
    public static ExceptionAgreementView ToExceptionAgreementView(ExceptionAgreement item)
    {
        return new ExceptionAgreementView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionAgreementView> ToExceptionAgreementViews(List<ExceptionAgreement> items)
    {
        var views = new List<ExceptionAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionAgreementView(item));
        }
        return views;
    }

    public static double SumExceptionAgreementView(ExceptionAgreementView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedRetries + (double)parsedAttempts;
    }
}
