namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg template.</summary>
public sealed record ReorgTemplateView(string Code, string NameText, string Rate, string Retries, string Label, string Grade, string FteText, string Locale, string Capacity, string Revision, string StatusLabel)
{
    public static ReorgTemplateView ToReorgTemplateView(ReorgTemplate item)
    {
        return new ReorgTemplateView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Grade,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgTemplateView> ToReorgTemplateViews(List<ReorgTemplate> items)
    {
        var views = new List<ReorgTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToReorgTemplateView(item));
        }
        return views;
    }

    public static double SumReorgTemplateView(ReorgTemplateView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedRetries + parsedFte;
    }
}
