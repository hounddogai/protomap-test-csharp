namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Functions of recognition profile from preference.</summary>
public static class RecognitionProfileFromPreference
{
    public static Dictionary<string, double> SummarizeLenientRecognitionPreferencesForProfile(List<LenientRecognitionPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<RecognitionProfile> DeriveRecognitionProfilesFromPreference(List<LenientRecognitionPreference> items)
    {
        var outValue = new List<RecognitionProfile>();
        foreach (var item in items)
        {
            var derived = RecognitionProfile.NewRecognitionProfile(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRecognitionProfile(RecognitionProfile item)
    {
        return LenientRecognitionPreferencePolicy.NormalizeLenientRecognitionPreferenceCode(item.Code);
    }
}
