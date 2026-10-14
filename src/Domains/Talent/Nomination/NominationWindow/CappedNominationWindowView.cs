namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped nomination window.</summary>
public sealed record CappedNominationWindowView(string Code, string Label, string Hours, string AmountText, string NameText, string StatusLabel)
{
    public static CappedNominationWindowView ToCappedNominationWindowView(CappedNominationWindow item)
    {
        return new CappedNominationWindowView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedNominationWindowView> ToCappedNominationWindowViews(List<CappedNominationWindow> items)
    {
        var views = new List<CappedNominationWindowView>();
        foreach (var item in items)
        {
            views.Add(ToCappedNominationWindowView(item));
        }
        return views;
    }

    public static double SumCappedNominationWindowView(CappedNominationWindowView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedHours + parsedAmount;
    }
}
