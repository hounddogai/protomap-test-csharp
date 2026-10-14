namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default statement of work audit.</summary>
public sealed record DefaultStatementOfWorkAuditView(string Code, string NameText, string DiscountText, string Cost, string BatchSize, string ReviewerText, string DescriptionText, string RetriesText, string RegionText, string StatusLabel)
{
    public static DefaultStatementOfWorkAuditView ToDefaultStatementOfWorkAuditView(DefaultStatementOfWorkAudit item)
    {
        return new DefaultStatementOfWorkAuditView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Description,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultStatementOfWorkAuditView> ToDefaultStatementOfWorkAuditViews(List<DefaultStatementOfWorkAudit> items)
    {
        var views = new List<DefaultStatementOfWorkAuditView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultStatementOfWorkAuditView(item));
        }
        return views;
    }

    public static double SumDefaultStatementOfWorkAuditView(DefaultStatementOfWorkAuditView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedCost + (double)parsedBatchSize;
    }
}
