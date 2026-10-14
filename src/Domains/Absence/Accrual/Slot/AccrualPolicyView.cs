namespace Acme.Workforce.Absence.Accrual.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual policy.</summary>
public sealed record AccrualPolicyView(string Code, string Label, string BatchSize, string Unit, string Minimum, string CeilingText, string Visible, string StatusLabel)
{
    public static AccrualPolicyView ToAccrualPolicyView(AccrualPolicy item)
    {
        return new AccrualPolicyView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualPolicyView> ToAccrualPolicyViews(List<AccrualPolicy> items)
    {
        var views = new List<AccrualPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualPolicyView(item));
        }
        return views;
    }

    public static double SumAccrualPolicyView(AccrualPolicyView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedMinimum + parsedCeiling;
    }
}
