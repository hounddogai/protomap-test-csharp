namespace Acme.Workforce.Talent.Pool.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool template.</summary>
public sealed record PoolTemplateView(string CodeText, string LabelText, string Source, string ManagerId, string ArchivedText, string AmountText, string StatusLabel)
{
    public static PoolTemplateView ToPoolTemplateView(PoolTemplate item)
    {
        return new PoolTemplateView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 40),
            item.ManagerId,
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolTemplateView> ToPoolTemplateViews(List<PoolTemplate> items)
    {
        var views = new List<PoolTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPoolTemplateView(item));
        }
        return views;
    }

    public static double SumPoolTemplateView(PoolTemplateView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
