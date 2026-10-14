namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default reorg group.</summary>
public sealed record DefaultReorgGroupView(string Code, string LabelText, string Fte, string UpdatedAt, string DiscountText, string StatusLabel)
{
    public static DefaultReorgGroupView ToDefaultReorgGroupView(DefaultReorgGroup item)
    {
        return new DefaultReorgGroupView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultReorgGroupView> ToDefaultReorgGroupViews(List<DefaultReorgGroup> items)
    {
        var views = new List<DefaultReorgGroupView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultReorgGroupView(item));
        }
        return views;
    }

    public static double SumDefaultReorgGroupView(DefaultReorgGroupView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedUpdatedAt + parsedDiscount;
    }
}
