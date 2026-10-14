namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy mapping must pass. Check returns its problems, or none.</summary>
public interface IPolicyMappingRule
{
    List<string> Check(PolicyMapping item);
}

public sealed record PolicyMappingRequiredNameRule(string Message) : IPolicyMappingRule
{
    public List<string> Check(PolicyMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyMappingVisibleGuardRule(int Limit) : IPolicyMappingRule
{
    public List<string> Check(PolicyMapping item)
    {
        if (item.Visible && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PolicyMappingUnitLengthRule(int MaxLength) : IPolicyMappingRule
{
    public List<string> Check(PolicyMapping item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyMappingRequiredCodeRule(string Message) : IPolicyMappingRule
{
    public List<string> Check(PolicyMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyMappingAllowedSummaryRule(List<string> Allowed) : IPolicyMappingRule
{
    public List<string> Check(PolicyMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy mapping.</summary>
public sealed record PolicyMappingValidator(List<IPolicyMappingRule> Rules)
{
    public List<string> Validate(PolicyMapping item)
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

    public void RequireValid(PolicyMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyMapping> items)
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

/// <summary>Functions of policy mapping rules.</summary>
public static class PolicyMappingRules
{
    public static PolicyMappingValidator DefaultPolicyMappingValidator()
    {
        return new PolicyMappingValidator(
            new List<IPolicyMappingRule>
            {
                new PolicyMappingRequiredNameRule("policy mapping needs an entry"),
                new PolicyMappingVisibleGuardRule(3600),
                new PolicyMappingUnitLengthRule(255),
                new PolicyMappingRequiredCodeRule("policy mapping needs an entry"),
                new PolicyMappingAllowedSummaryRule(new List<string> { "capped", "adaptive" }),
            });
    }
}
