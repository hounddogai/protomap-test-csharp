namespace Acme.Workforce.Talent.Nomination.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination cycle.</summary>
public sealed record NominationCycleView(string Code, string TitleText, string Country, string Label, string Cost, string ExpiresAt, string Retries, string Phone, string StatusLabel)
{
    public static NominationCycleView ToNominationCycleView(NominationCycle item)
    {
        return new NominationCycleView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Country,
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationCycleView> ToNominationCycleViews(List<NominationCycle> items)
    {
        var views = new List<NominationCycleView>();
        foreach (var item in items)
        {
            views.Add(ToNominationCycleView(item));
        }
        return views;
    }

    public static double SumNominationCycleView(NominationCycleView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedExpiresAt + (double)parsedRetries;
    }
}
