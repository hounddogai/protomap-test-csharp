namespace Acme.Workforce.Compliance.Exception.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception draft.</summary>
public sealed record ExceptionDraftView(string Code, string Title, string Skill, string ExpiresAt, string RetriesText, string Unit, string Description, string Limit, string SequenceText, string TeamText, string StatusLabel)
{
    public static ExceptionDraftView ToExceptionDraftView(ExceptionDraft item)
    {
        return new ExceptionDraftView(
            item.Code,
            item.Title,
            item.Skill,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            item.Description,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionDraftView> ToExceptionDraftViews(List<ExceptionDraft> items)
    {
        var views = new List<ExceptionDraftView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionDraftView(item));
        }
        return views;
    }

    public static double SumExceptionDraftView(ExceptionDraftView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedRetries + (double)parsedLimit;
    }
}
