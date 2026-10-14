namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Functions of seasonal access badge draft from manifest.</summary>
public static class SeasonalAccessBadgeDraftFromManifest
{
    public static Dictionary<string, double> SummarizeBookingManifestsForDraft(List<BookingManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<SeasonalAccessBadgeDraft> DeriveSeasonalAccessBadgeDraftsFromManifest(List<BookingManifest> items)
    {
        var outValue = new List<SeasonalAccessBadgeDraft>();
        foreach (var item in items)
        {
            var derived = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceSeasonalAccessBadgeDraft(SeasonalAccessBadgeDraft item)
    {
        return BookingManifestPolicy.ClassifyBookingManifest(item.MeasuredValue());
    }
}
