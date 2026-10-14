namespace Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>A rule a return plan quota must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanQuotaRule
{
    List<string> Check(ReturnPlanQuota item);
}

public sealed record ReturnPlanQuotaRevisionRangeRule(int Min, int Max) : IReturnPlanQuotaRule
{
    public List<string> Check(ReturnPlanQuota item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanQuotaRequiredCodeRule(string Message) : IReturnPlanQuotaRule
{
    public List<string> Check(ReturnPlanQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanQuotaCodePrefixRule(string Prefix) : IReturnPlanQuotaRule
{
    public List<string> Check(ReturnPlanQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanQuotaCapacityRangeRule(int Min, int Max) : IReturnPlanQuotaRule
{
    public List<string> Check(ReturnPlanQuota item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanQuotaRequiredCostCenterRule(string Message) : IReturnPlanQuotaRule
{
    public List<string> Check(ReturnPlanQuota item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan quota.</summary>
public sealed record ReturnPlanQuotaValidator(List<IReturnPlanQuotaRule> Rules)
{
    public List<string> Validate(ReturnPlanQuota item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(ReturnPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanQuota> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of return plan quota rules.</summary>
public static class ReturnPlanQuotaRules
{
    public static ReturnPlanQuotaValidator DefaultReturnPlanQuotaValidator()
    {
        return new ReturnPlanQuotaValidator(
            new List<IReturnPlanQuotaRule>
            {
                new ReturnPlanQuotaRevisionRangeRule(80, 3600),
                new ReturnPlanQuotaRequiredCodeRule("return plan quota needs a value"),
                new ReturnPlanQuotaCodePrefixRule("X"),
                new ReturnPlanQuotaCapacityRangeRule(32, 255),
                new ReturnPlanQuotaRequiredCostCenterRule("return plan quota needs an entry"),
            });
    }
}
