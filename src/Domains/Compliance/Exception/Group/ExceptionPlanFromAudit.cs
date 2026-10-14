namespace Acme.Workforce.Compliance.Exception.Group;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Functions of exception plan from audit.</summary>
public static class ExceptionPlanFromAudit
{
    public static Dictionary<string, double> SummarizeCertificationAuditsForPlan(List<CertificationAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ExceptionPlan> DeriveExceptionPlansFromAudit(List<CertificationAudit> items)
    {
        var outValue = new List<ExceptionPlan>();
        foreach (var item in items)
        {
            var derived = ExceptionPlan.NewExceptionPlan(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignExceptionPlan(ExceptionPlan item)
    {
        return CertificationOverridePolicy.NormalizeCertificationOverrideCode(item.Code);
    }
}
