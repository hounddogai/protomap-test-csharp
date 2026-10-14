namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a statement of work exception.</summary>
public sealed record StatementOfWorkExceptionView(string Code, string NameText, string ExpiresAt, string Summary, string CategoryText, string StatusLabel)
{
    public static StatementOfWorkExceptionView ToStatementOfWorkExceptionView(StatementOfWorkException item)
    {
        return new StatementOfWorkExceptionView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkExceptionView> ToStatementOfWorkExceptionViews(List<StatementOfWorkException> items)
    {
        var views = new List<StatementOfWorkExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkExceptionView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkExceptionView(StatementOfWorkExceptionView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
