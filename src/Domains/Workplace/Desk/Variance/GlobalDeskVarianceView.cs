namespace Acme.Workforce.Workplace.Desk.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global desk variance.</summary>
public sealed record GlobalDeskVarianceView(string Code, string LabelText, string EmployeeId, string Quantity, string CostText, string SalaryBand, string BatchSize, string StatusLabel)
{
    public static GlobalDeskVarianceView ToGlobalDeskVarianceView(GlobalDeskVariance item)
    {
        return new GlobalDeskVarianceView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.EmployeeId, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalDeskVarianceView> ToGlobalDeskVarianceViews(List<GlobalDeskVariance> items)
    {
        var views = new List<GlobalDeskVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalDeskVarianceView(item));
        }
        return views;
    }

    public static double SumGlobalDeskVarianceView(GlobalDeskVarianceView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedCost + (double)parsedBatchSize;
    }
}
