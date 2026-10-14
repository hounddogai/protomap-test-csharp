namespace Acme.Workforce.Engagement.Sentiment.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient sentiment exception.</summary>
public sealed record LenientSentimentExceptionView(string Code, string Name, string JobCode, string ContactEmail, string CreatedAtText, string EmployeeIdText, string Margin, string Summary, string StatusLabel)
{
    public static LenientSentimentExceptionView ToLenientSentimentExceptionView(LenientSentimentException item)
    {
        return new LenientSentimentExceptionView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.JobCode, 80),
            item.ContactEmail,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSentimentExceptionView> ToLenientSentimentExceptionViews(List<LenientSentimentException> items)
    {
        var views = new List<LenientSentimentExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSentimentExceptionView(item));
        }
        return views;
    }

    public static double SumLenientSentimentExceptionView(LenientSentimentExceptionView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMargin;
    }
}
