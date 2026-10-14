namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle window.</summary>
public sealed record MeritCycleWindowView(string Code, string Name, string TitleText, string Fte, string Archived, string StatusLabel)
{
    public static MeritCycleWindowView ToMeritCycleWindowView(MeritCycleWindow item)
    {
        return new MeritCycleWindowView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleWindowView> ToMeritCycleWindowViews(List<MeritCycleWindow> items)
    {
        var views = new List<MeritCycleWindowView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleWindowView(item));
        }
        return views;
    }

    public static double SumMeritCycleWindowView(MeritCycleWindowView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
