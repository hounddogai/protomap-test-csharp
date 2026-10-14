namespace Acme.Workforce.Contractors.Rate.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive rate template.</summary>
public sealed record AdaptiveRateTemplateView(string Code, string LabelText, string Phone, string Channel, string RateText, string StatusLabel)
{
    public static AdaptiveRateTemplateView ToAdaptiveRateTemplateView(AdaptiveRateTemplate item)
    {
        return new AdaptiveRateTemplateView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Phone,
            item.Channel,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveRateTemplateView> ToAdaptiveRateTemplateViews(List<AdaptiveRateTemplate> items)
    {
        var views = new List<AdaptiveRateTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveRateTemplateView(item));
        }
        return views;
    }

    public static double SumAdaptiveRateTemplateView(AdaptiveRateTemplateView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
