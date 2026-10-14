namespace Acme.Workforce.Talent.Nomination.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination claim.</summary>
public sealed record NominationClaimView(string Code, string Title, string StartsAtText, string PinnedText, string LocationText, string Attempts, string StatusLabel)
{
    public static NominationClaimView ToNominationClaimView(NominationClaim item)
    {
        return new NominationClaimView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Location,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationClaimView> ToNominationClaimViews(List<NominationClaim> items)
    {
        var views = new List<NominationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToNominationClaimView(item));
        }
        return views;
    }

    public static double SumNominationClaimView(NominationClaimView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedAttempts;
    }
}
