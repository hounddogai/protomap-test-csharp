namespace Acme.Workforce.Orgdesign.Position.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled position template.</summary>
public sealed record ScheduledPositionTemplateView(string CodeText, string TitleText, string SummaryText, string Retries, string Unit, string StatusLabel)
{
    public static ScheduledPositionTemplateView ToScheduledPositionTemplateView(ScheduledPositionTemplate item)
    {
        return new ScheduledPositionTemplateView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.Summary, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPositionTemplateView> ToScheduledPositionTemplateViews(List<ScheduledPositionTemplate> items)
    {
        var views = new List<ScheduledPositionTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPositionTemplateView(item));
        }
        return views;
    }

    public static double SumScheduledPositionTemplateView(ScheduledPositionTemplateView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
