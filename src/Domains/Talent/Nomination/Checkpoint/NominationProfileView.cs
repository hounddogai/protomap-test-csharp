namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination profile.</summary>
public sealed record NominationProfileView(string Code, string Title, string Minimum, string Label, string BudgetText, string ReviewerText, string Amount, string StatusLabel)
{
    public static NominationProfileView ToNominationProfileView(NominationProfile item)
    {
        return new NominationProfileView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationProfileView> ToNominationProfileViews(List<NominationProfile> items)
    {
        var views = new List<NominationProfileView>();
        foreach (var item in items)
        {
            views.Add(ToNominationProfileView(item));
        }
        return views;
    }

    public static double SumNominationProfileView(NominationProfileView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget + parsedAmount;
    }
}
