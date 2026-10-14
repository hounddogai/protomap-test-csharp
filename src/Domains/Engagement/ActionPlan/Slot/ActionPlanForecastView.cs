namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using System.Globalization;

/// <summary>How screens and exports show a action plan forecast.</summary>
public sealed record ActionPlanForecastView(string CodeText, string Title, string ContactEmail, string VersionText, string Rate, string Phone, string Currency, string Name, string LocationText, string StatusLabel)
{
    public static ActionPlanForecastView ToActionPlanForecastView(ActionPlanForecast item)
    {
        return new ActionPlanForecastView(
            item.Code,
            item.Title,
            item.ContactEmail,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Currency,
            item.Name,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanForecastView> ToActionPlanForecastViews(List<ActionPlanForecast> items)
    {
        var views = new List<ActionPlanForecastView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanForecastView(item));
        }
        return views;
    }

    public static double SumActionPlanForecastView(ActionPlanForecastView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRate;
    }
}
