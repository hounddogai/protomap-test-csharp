namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Functions of lenient nomination request from reservation.</summary>
public static class LenientNominationRequestFromReservation
{
    public static Dictionary<string, double> SummarizeRollingSkillProfileReservationsForRequest(List<RollingSkillProfileReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<LenientNominationRequest> DeriveLenientNominationRequestsFromReservation(List<RollingSkillProfileReservation> items)
    {
        var outValue = new List<LenientNominationRequest>();
        foreach (var item in items)
        {
            var derived = LenientNominationRequest.NewLenientNominationRequest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLenientNominationRequest(LenientNominationRequest item)
    {
        return SeasonalSkillProfilePreferencePolicy.ClassifySeasonalSkillProfilePreference(item.NetLoad());
    }
}
