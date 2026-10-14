namespace Acme.Workforce.Absence.Accrual.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual accrual policy.</summary>
public sealed record ManualAccrualPolicyView(string Code, string Name, string ExternalRef, string ExpiresAt, string Badge, string Source, string QuantityText, string StatusLabel)
{
    public static ManualAccrualPolicyView ToManualAccrualPolicyView(ManualAccrualPolicy item)
    {
        return new ManualAccrualPolicyView(
            item.Code,
            item.Name,
            item.ExternalRef,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.Source,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccrualPolicyView> ToManualAccrualPolicyViews(List<ManualAccrualPolicy> items)
    {
        var views = new List<ManualAccrualPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccrualPolicyView(item));
        }
        return views;
    }

    public static double SumManualAccrualPolicyView(ManualAccrualPolicyView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedQuantity;
    }
}
