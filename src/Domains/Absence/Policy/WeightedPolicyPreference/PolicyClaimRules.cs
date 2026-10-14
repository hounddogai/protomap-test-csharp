namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>A rule a policy claim must pass. Check returns its problems, or none.</summary>
public interface IPolicyClaimRule
{
    List<string> Check(PolicyClaim item);
}

public sealed record PolicyClaimWeightRangeRule(double Min, double Max) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyClaimCostRangeRule(double Min, double Max) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyClaimCodePrefixRule(string Prefix) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyClaimCategoryLengthRule(int MaxLength) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyClaimRequiredTeamRule(string Message) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyClaimRequiredCategoryRule(string Message) : IPolicyClaimRule
{
    public List<string> Check(PolicyClaim item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy claim.</summary>
public sealed record PolicyClaimValidator(List<IPolicyClaimRule> Rules)
{
    public List<string> Validate(PolicyClaim item)
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

    public void RequireValid(PolicyClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyClaim> items)
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

/// <summary>Functions of policy claim rules.</summary>
public static class PolicyClaimRules
{
    public static PolicyClaimValidator DefaultPolicyClaimValidator()
    {
        return new PolicyClaimValidator(
            new List<IPolicyClaimRule>
            {
                new PolicyClaimWeightRangeRule(6.6, 4720.8),
                new PolicyClaimCostRangeRule(7.1, 4490.7),
                new PolicyClaimCodePrefixRule("OPS"),
                new PolicyClaimCategoryLengthRule(32),
                new PolicyClaimRequiredTeamRule("policy claim needs an entry"),
                new PolicyClaimRequiredCategoryRule("policy claim needs a value"),
            });
    }
}
