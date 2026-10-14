namespace Acme.Workforce.Payrollops.Garnishment.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment template.</summary>
public sealed record GarnishmentTemplateView(string CodeText, string Title, string Priority, string ScoreText, string LabelText, string ExternalRefText, string Discount, string Badge, string StatusLabel)
{
    public static GarnishmentTemplateView ToGarnishmentTemplateView(GarnishmentTemplate item)
    {
        return new GarnishmentTemplateView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 80),
            item.ExternalRef,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentTemplateView> ToGarnishmentTemplateViews(List<GarnishmentTemplate> items)
    {
        var views = new List<GarnishmentTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentTemplateView(item));
        }
        return views;
    }

    public static double SumGarnishmentTemplateView(GarnishmentTemplateView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedScore + parsedDiscount;
    }
}
