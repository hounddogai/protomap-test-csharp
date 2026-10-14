namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Functions of lenient sentiment exception from group.</summary>
public static class LenientSentimentExceptionFromGroup
{
    public static Dictionary<string, double> SummarizeRecognitionGroupsForException(List<RecognitionGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LenientSentimentException> DeriveLenientSentimentExceptionsFromGroup(List<RecognitionGroup> items)
    {
        var outValue = new List<LenientSentimentException>();
        foreach (var item in items)
        {
            var derived = LenientSentimentException.NewLenientSentimentException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLenientSentimentException(LenientSentimentException item)
    {
        return RecognitionClaimPolicy.RecognitionClaimBucket(item.NetValue());
    }
}
