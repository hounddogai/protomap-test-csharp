namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using System.Globalization;

/// <summary>How screens and exports show a adaptive pay run allocation.</summary>
public sealed record AdaptivePayRunAllocationView(string CodeText, string Title, string Summary, string Rate, string StatusLabel)
{
    public static AdaptivePayRunAllocationView ToAdaptivePayRunAllocationView(AdaptivePayRunAllocation item)
    {
        return new AdaptivePayRunAllocationView(
            item.Code,
            item.Title,
            item.Summary,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePayRunAllocationView> ToAdaptivePayRunAllocationViews(List<AdaptivePayRunAllocation> items)
    {
        var views = new List<AdaptivePayRunAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePayRunAllocationView(item));
        }
        return views;
    }

    public static double SumAdaptivePayRunAllocationView(AdaptivePayRunAllocationView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
