namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work request.</summary>
public sealed record StatementOfWorkRequestView(string CodeText, string Title, string SalaryBand, string Team, string Timezone, string AttemptsText, string Channel, string LocaleText, string StatusLabel)
{
    public static StatementOfWorkRequestView ToStatementOfWorkRequestView(StatementOfWorkRequest item)
    {
        return new StatementOfWorkRequestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.SalaryBand, 40),
            Text.Truncate(item.Team, 40),
            item.Timezone,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 120),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkRequestView> ToStatementOfWorkRequestViews(List<StatementOfWorkRequest> items)
    {
        var views = new List<StatementOfWorkRequestView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkRequestView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkRequestView(StatementOfWorkRequestView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
