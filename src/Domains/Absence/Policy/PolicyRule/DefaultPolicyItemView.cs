namespace Acme.Workforce.Absence.Policy.PolicyRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default policy item.</summary>
public sealed record DefaultPolicyItemView(string CodeText, string Name, string NotesText, string Retries, string Capacity, string ExternalRefText, string Amount, string RevisionText, string GraceDaysText, string ApprovedText, string StatusLabel)
{
    public static DefaultPolicyItemView ToDefaultPolicyItemView(DefaultPolicyItem item)
    {
        return new DefaultPolicyItemView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Notes,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPolicyItemView> ToDefaultPolicyItemViews(List<DefaultPolicyItem> items)
    {
        var views = new List<DefaultPolicyItemView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPolicyItemView(item));
        }
        return views;
    }

    public static double SumDefaultPolicyItemView(DefaultPolicyItemView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedCapacity + parsedAmount;
    }
}
