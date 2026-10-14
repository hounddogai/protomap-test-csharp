namespace Acme.Workforce.Engagement.TownHall.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall exception.</summary>
public sealed record TownHallExceptionView(string CodeText, string Name, string Unit, string Amount, string Version, string ActiveText, string Timezone, string Notes, string Retries, string StatusLabel)
{
    public static TownHallExceptionView ToTownHallExceptionView(TownHallException item)
    {
        return new TownHallExceptionView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Unit, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Timezone,
            item.Notes,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallExceptionView> ToTownHallExceptionViews(List<TownHallException> items)
    {
        var views = new List<TownHallExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallExceptionView(item));
        }
        return views;
    }

    public static double SumTownHallExceptionView(TownHallExceptionView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedVersion + (double)parsedRetries;
    }
}
