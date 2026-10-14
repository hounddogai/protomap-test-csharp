namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Functions of default engagement exception from target.</summary>
public static class DefaultEngagementExceptionFromTarget
{
    public static Dictionary<string, double> SummarizeLegacyStatementOfWorkTargetsForException(List<LegacyStatementOfWorkTarget> items)
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

    public static List<DefaultEngagementException> DeriveDefaultEngagementExceptionsFromTarget(List<LegacyStatementOfWorkTarget> items)
    {
        var outValue = new List<DefaultEngagementException>();
        foreach (var item in items)
        {
            var derived = DefaultEngagementException.NewDefaultEngagementException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDefaultEngagementException(DefaultEngagementException item)
    {
        return AdaptiveStatementOfWorkAuditPolicy.NormalizeAdaptiveStatementOfWorkAuditCode(item.Code);
    }
}
