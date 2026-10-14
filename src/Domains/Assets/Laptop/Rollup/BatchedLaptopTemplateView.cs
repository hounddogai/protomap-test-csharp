namespace Acme.Workforce.Assets.Laptop.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a batched laptop template.</summary>
public sealed record BatchedLaptopTemplateView(string CodeText, string Title, string Source, string CapacityText, string Priority, string StatusLabel)
{
    public static BatchedLaptopTemplateView ToBatchedLaptopTemplateView(BatchedLaptopTemplate item)
    {
        return new BatchedLaptopTemplateView(
            item.Code,
            item.Title,
            item.Source,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedLaptopTemplateView> ToBatchedLaptopTemplateViews(List<BatchedLaptopTemplate> items)
    {
        var views = new List<BatchedLaptopTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedLaptopTemplateView(item));
        }
        return views;
    }

    public static double SumBatchedLaptopTemplateView(BatchedLaptopTemplateView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedPriority;
    }
}
