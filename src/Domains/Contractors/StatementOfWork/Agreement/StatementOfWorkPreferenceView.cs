namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work preference.</summary>
public sealed record StatementOfWorkPreferenceView(string Code, string Name, string PhoneText, string Fte, string Limit, string UpdatedAtText, string SalaryBand, string ArchivedText, string JobCode, string StatusLabel)
{
    public static StatementOfWorkPreferenceView ToStatementOfWorkPreferenceView(StatementOfWorkPreference item)
    {
        return new StatementOfWorkPreferenceView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Phone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            $"{item.Archived}",
            Text.Truncate(item.JobCode, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkPreferenceView> ToStatementOfWorkPreferenceViews(List<StatementOfWorkPreference> items)
    {
        var views = new List<StatementOfWorkPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkPreferenceView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkPreferenceView(StatementOfWorkPreferenceView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLimit + (double)parsedUpdatedAt;
    }
}
