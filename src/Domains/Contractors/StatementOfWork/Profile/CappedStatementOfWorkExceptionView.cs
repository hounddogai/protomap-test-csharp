namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped statement of work exception.</summary>
public sealed record CappedStatementOfWorkExceptionView(string Code, string Name, string Rate, string CostCenterText, string Channel, string CreatedAt, string Discount, string EmployeeId, string StatusLabel)
{
    public static CappedStatementOfWorkExceptionView ToCappedStatementOfWorkExceptionView(CappedStatementOfWorkException item)
    {
        return new CappedStatementOfWorkExceptionView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Channel,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedStatementOfWorkExceptionView> ToCappedStatementOfWorkExceptionViews(List<CappedStatementOfWorkException> items)
    {
        var views = new List<CappedStatementOfWorkExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToCappedStatementOfWorkExceptionView(item));
        }
        return views;
    }

    public static double SumCappedStatementOfWorkExceptionView(CappedStatementOfWorkExceptionView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedCreatedAt + parsedDiscount;
    }
}
