namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of primary exception request from adjustment.</summary>
public static class PrimaryExceptionRequestFromAdjustment
{
    public static Dictionary<string, double> SummarizeCertificationAdjustmentsForRequest(List<CertificationAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<PrimaryExceptionRequest> DerivePrimaryExceptionRequestsFromAdjustment(List<CertificationAdjustment> items)
    {
        var outValue = new List<PrimaryExceptionRequest>();
        foreach (var item in items)
        {
            var derived = PrimaryExceptionRequest.NewPrimaryExceptionRequest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPrimaryExceptionRequest(PrimaryExceptionRequest item)
    {
        return CertificationRequestPolicy.CertificationRequestBucket(item.MeasuredValue());
    }
}
