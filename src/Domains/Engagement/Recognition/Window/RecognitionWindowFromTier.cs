namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Functions of recognition window from tier.</summary>
public static class RecognitionWindowFromTier
{
    public static Dictionary<string, double> SummarizeStrictPulseSurveyTiersForWindow(List<StrictPulseSurveyTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<RecognitionWindow> DeriveRecognitionWindowsFromTier(List<StrictPulseSurveyTier> items)
    {
        var outValue = new List<RecognitionWindow>();
        foreach (var item in items)
        {
            var derived = RecognitionWindow.NewRecognitionWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRecognitionWindow(RecognitionWindow item)
    {
        return PulseSurveySlotPolicy.ClassifyPulseSurveySlot(item.WeightedValue());
    }
}
