namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Functions of fallback exception assignment from cycle.</summary>
public static class FallbackExceptionAssignmentFromCycle
{
    public static Dictionary<string, double> SummarizeCertificationCyclesForAssignment(List<CertificationCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<FallbackExceptionAssignment> DeriveFallbackExceptionAssignmentsFromCycle(List<CertificationCycle> items)
    {
        var outValue = new List<FallbackExceptionAssignment>();
        foreach (var item in items)
        {
            var derived = FallbackExceptionAssignment.NewFallbackExceptionAssignment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyFallbackExceptionAssignment(FallbackExceptionAssignment item)
    {
        return CertificationBundlePolicy.ClassifyCertificationBundle(item.AdjustedVolume());
    }
}
