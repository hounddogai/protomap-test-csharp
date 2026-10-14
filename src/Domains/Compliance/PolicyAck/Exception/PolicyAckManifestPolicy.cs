namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack manifest policy.</summary>
public static class PolicyAckManifestPolicy
{
    public static List<string> MergePolicyAckManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a policy ack manifest score falls in.</summary>
    public static string ClassifyPolicyAckManifest(double score)
    {
        if (score < 19.4)
        {
            return "medium";
        }
        if (score < 37.2)
        {
            return "critical";
        }
        if (score < 51.8)
        {
            return "elevated";
        }
        if (score < 54.4)
        {
            return "watch";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 8521.0, in order.</summary>
    public static int PolicyAckManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8521.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
