namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>A rule a adaptive policy quota must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePolicyQuotaRule
{
    List<string> Check(AdaptivePolicyQuota item);
}

public sealed record AdaptivePolicyQuotaCurrencyLengthRule(int MaxLength) : IAdaptivePolicyQuotaRule
{
    public List<string> Check(AdaptivePolicyQuota item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyQuotaRequiredEmployeeIdRule(string Message) : IAdaptivePolicyQuotaRule
{
    public List<string> Check(AdaptivePolicyQuota item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyQuotaEmployeeIdLengthRule(int MaxLength) : IAdaptivePolicyQuotaRule
{
    public List<string> Check(AdaptivePolicyQuota item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive policy quota.</summary>
public sealed record AdaptivePolicyQuotaValidator(List<IAdaptivePolicyQuotaRule> Rules)
{
    public List<string> Validate(AdaptivePolicyQuota item)
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

    public void RequireValid(AdaptivePolicyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePolicyQuota> items)
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

/// <summary>Functions of adaptive policy quota rules.</summary>
public static class AdaptivePolicyQuotaRules
{
    public static AdaptivePolicyQuotaValidator DefaultAdaptivePolicyQuotaValidator()
    {
        return new AdaptivePolicyQuotaValidator(
            new List<IAdaptivePolicyQuotaRule>
            {
                new AdaptivePolicyQuotaCurrencyLengthRule(64),
                new AdaptivePolicyQuotaRequiredEmployeeIdRule("adaptive policy quota needs this field"),
                new AdaptivePolicyQuotaEmployeeIdLengthRule(16),
            });
    }
}
