namespace Acme.Workforce.Engagement.TownHall.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall template.</summary>
public sealed record TownHallTemplateView(string CodeText, string Label, string SummaryText, string CostText, string Sequence, string NotesText, string EndsAt, string StatusLabel)
{
    public static TownHallTemplateView ToTownHallTemplateView(TownHallTemplate item)
    {
        return new TownHallTemplateView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Summary,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallTemplateView> ToTownHallTemplateViews(List<TownHallTemplate> items)
    {
        var views = new List<TownHallTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallTemplateView(item));
        }
        return views;
    }

    public static double SumTownHallTemplateView(TownHallTemplateView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedSequence + (double)parsedEndsAt;
    }
}
