namespace Acme.Workforce.Talent.Pool.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool entry.</summary>
public sealed record PoolEntryView(string CodeText, string Name, string Capacity, string Priority, string Timezone, string Discount, string Amount, string CreatedAtText, string Department, string BatchSizeText, string Reviewer, string StatusLabel)
{
    public static PoolEntryView ToPoolEntryView(PoolEntry item)
    {
        return new PoolEntryView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolEntryView> ToPoolEntryViews(List<PoolEntry> items)
    {
        var views = new List<PoolEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPoolEntryView(item));
        }
        return views;
    }

    public static double SumPoolEntryView(PoolEntryView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedPriority + parsedDiscount;
    }
}
