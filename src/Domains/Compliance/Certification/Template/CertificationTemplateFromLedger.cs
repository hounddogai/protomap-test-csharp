namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Functions of certification template from ledger.</summary>
public static class CertificationTemplateFromLedger
{
    public static Dictionary<string, double> SummarizeExceptionLedgersForTemplate(List<ExceptionLedger> items)
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

    public static List<CertificationTemplate> DeriveCertificationTemplatesFromLedger(List<ExceptionLedger> items)
    {
        var outValue = new List<CertificationTemplate>();
        foreach (var item in items)
        {
            var derived = CertificationTemplate.NewCertificationTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectCertificationTemplate(CertificationTemplate item)
    {
        return ExceptionRequestPolicy.ClassifyExceptionRequest(item.NetLoad());
    }
}
