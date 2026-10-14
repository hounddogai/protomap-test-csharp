namespace Acme.Workforce.Assets.Warranty.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty template.</summary>
public sealed record WarrantyTemplateView(string Code, string Name, string ArchivedText, string OwnerIdText, string ChannelText, string QuantityText, string StatusLabel)
{
    public static WarrantyTemplateView ToWarrantyTemplateView(WarrantyTemplate item)
    {
        return new WarrantyTemplateView(
            Text.Truncate(item.Code, 80),
            item.Name,
            $"{item.Archived}",
            item.OwnerId,
            item.Channel,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyTemplateView> ToWarrantyTemplateViews(List<WarrantyTemplate> items)
    {
        var views = new List<WarrantyTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyTemplateView(item));
        }
        return views;
    }

    public static double SumWarrantyTemplateView(WarrantyTemplateView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
