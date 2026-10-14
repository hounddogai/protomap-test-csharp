namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient recognition entry policy.</summary>
public static class LenientRecognitionEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48189.0, in order.</summary>
    public static int LenientRecognitionEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48189.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLenientRecognitionEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
