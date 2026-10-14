namespace Acme.Workforce.Contractors.Rate.RateNotice;

using System.Globalization;

/// <summary>How screens and exports show a capped rate proposal.</summary>
public sealed record CappedRateProposalView(string Code, string Name, string ContactEmailText, string DurationMinutesText, string EndsAt, string Limit, string Active, string Margin, string Priority, string StatusLabel)
{
    public static CappedRateProposalView ToCappedRateProposalView(CappedRateProposal item)
    {
        return new CappedRateProposalView(
            item.Code,
            item.Name,
            item.ContactEmail,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRateProposalView> ToCappedRateProposalViews(List<CappedRateProposal> items)
    {
        var views = new List<CappedRateProposalView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRateProposalView(item));
        }
        return views;
    }

    public static double SumCappedRateProposalView(CappedRateProposalView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedEndsAt + (double)parsedLimit;
    }
}
