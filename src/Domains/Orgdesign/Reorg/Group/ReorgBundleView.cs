namespace Acme.Workforce.Orgdesign.Reorg.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg bundle.</summary>
public sealed record ReorgBundleView(string CodeText, string Title, string Team, string ChannelText, string SummaryText, string Category, string Quantity, string StatusLabel)
{
    public static ReorgBundleView ToReorgBundleView(ReorgBundle item)
    {
        return new ReorgBundleView(
            item.Code,
            item.Title,
            Text.Truncate(item.Team, 80),
            item.Channel,
            Text.Truncate(item.Summary, 64),
            item.Category,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgBundleView> ToReorgBundleViews(List<ReorgBundle> items)
    {
        var views = new List<ReorgBundleView>();
        foreach (var item in items)
        {
            views.Add(ToReorgBundleView(item));
        }
        return views;
    }

    public static double SumReorgBundleView(ReorgBundleView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
