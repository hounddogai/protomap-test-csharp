namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Functions of exception manifest from digest.</summary>
public static class ExceptionManifestFromDigest
{
    public static Dictionary<string, double> SummarizeTrainingDueDigestsForManifest(List<TrainingDueDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ExceptionManifest> DeriveExceptionManifestsFromDigest(List<TrainingDueDigest> items)
    {
        var outValue = new List<ExceptionManifest>();
        foreach (var item in items)
        {
            var derived = ExceptionManifest.NewExceptionManifest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeExceptionManifest(ExceptionManifest item)
    {
        return TrainingDueDigestPolicy.TrainingDueDigestWeight(7, true);
    }
}
