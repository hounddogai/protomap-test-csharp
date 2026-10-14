namespace Acme.Workforce.Absence.Policy.Group;

/// <summary>A rule a fallback policy quota must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyQuotaRule
{
    List<string> Check(FallbackPolicyQuota item);
}

public sealed record FallbackPolicyQuotaRequiredJobCodeRule(string Message) : IFallbackPolicyQuotaRule
{
    public List<string> Check(FallbackPolicyQuota item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyQuotaDiscountRangeRule(double Min, double Max) : IFallbackPolicyQuotaRule
{
    public List<string> Check(FallbackPolicyQuota item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPolicyQuotaNameLengthRule(int MaxLength) : IFallbackPolicyQuotaRule
{
    public List<string> Check(FallbackPolicyQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyQuotaRequiredCodeRule(string Message) : IFallbackPolicyQuotaRule
{
    public List<string> Check(FallbackPolicyQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy quota.</summary>
public sealed record FallbackPolicyQuotaValidator(List<IFallbackPolicyQuotaRule> Rules)
{
    public List<string> Validate(FallbackPolicyQuota item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(FallbackPolicyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyQuota> items)
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

/// <summary>Functions of fallback policy quota rules.</summary>
public static class FallbackPolicyQuotaRules
{
    public static FallbackPolicyQuotaValidator DefaultFallbackPolicyQuotaValidator()
    {
        return new FallbackPolicyQuotaValidator(
            new List<IFallbackPolicyQuotaRule>
            {
                new FallbackPolicyQuotaRequiredJobCodeRule("fallback policy quota needs this field"),
                new FallbackPolicyQuotaDiscountRangeRule(8.3, 1801.7),
                new FallbackPolicyQuotaNameLengthRule(16),
                new FallbackPolicyQuotaRequiredCodeRule("fallback policy quota needs an entry"),
            });
    }
}
