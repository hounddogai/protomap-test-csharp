namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary statement of work window.</summary>
public sealed record PrimaryStatementOfWorkWindowView(string Code, string TitleText, string Amount, string ContactEmail, string Cost, string CostCenter, string Source, string SalaryBand, string StatusLabel)
{
    public static PrimaryStatementOfWorkWindowView ToPrimaryStatementOfWorkWindowView(PrimaryStatementOfWorkWindow item)
    {
        return new PrimaryStatementOfWorkWindowView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Source,
            Text.Truncate(item.SalaryBand, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryStatementOfWorkWindowView> ToPrimaryStatementOfWorkWindowViews(List<PrimaryStatementOfWorkWindow> items)
    {
        var views = new List<PrimaryStatementOfWorkWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryStatementOfWorkWindowView(item));
        }
        return views;
    }

    public static double SumPrimaryStatementOfWorkWindowView(PrimaryStatementOfWorkWindowView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedAmount + parsedCost;
    }
}
