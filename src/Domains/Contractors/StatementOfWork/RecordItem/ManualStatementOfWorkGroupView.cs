namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual statement of work group.</summary>
public sealed record ManualStatementOfWorkGroupView(string Code, string Label, string Rate, string Summary, string CostCenter, string Amount, string StatusLabel)
{
    public static ManualStatementOfWorkGroupView ToManualStatementOfWorkGroupView(ManualStatementOfWorkGroup item)
    {
        return new ManualStatementOfWorkGroupView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            Text.Truncate(item.CostCenter, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualStatementOfWorkGroupView> ToManualStatementOfWorkGroupViews(List<ManualStatementOfWorkGroup> items)
    {
        var views = new List<ManualStatementOfWorkGroupView>();
        foreach (var item in items)
        {
            views.Add(ToManualStatementOfWorkGroupView(item));
        }
        return views;
    }

    public static double SumManualStatementOfWorkGroupView(ManualStatementOfWorkGroupView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedRate + parsedAmount;
    }
}
