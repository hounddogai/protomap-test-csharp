namespace Acme.Workforce.Engagement.TownHall.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient town hall target.</summary>
public sealed record LenientTownHallTargetView(string Code, string Name, string Skill, string LocationText, string Approved, string Fte, string StatusLabel)
{
    public static LenientTownHallTargetView ToLenientTownHallTargetView(LenientTownHallTarget item)
    {
        return new LenientTownHallTargetView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Skill,
            Text.Truncate(item.Location, 64),
            $"{item.Approved}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientTownHallTargetView> ToLenientTownHallTargetViews(List<LenientTownHallTarget> items)
    {
        var views = new List<LenientTownHallTargetView>();
        foreach (var item in items)
        {
            views.Add(ToLenientTownHallTargetView(item));
        }
        return views;
    }

    public static double SumLenientTownHallTargetView(LenientTownHallTargetView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
