namespace Acme.Workforce.Absence.Policy.Profile;

/// <summary>A rule a policy profile must pass. Check returns its problems, or none.</summary>
public interface IPolicyProfileRule
{
    List<string> Check(PolicyProfile item);
}

public sealed record PolicyProfileGraceDaysRangeRule(int Min, int Max) : IPolicyProfileRule
{
    public List<string> Check(PolicyProfile item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyProfileRequiredCostCenterRule(string Message) : IPolicyProfileRule
{
    public List<string> Check(PolicyProfile item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyProfileCategoryLengthRule(int MaxLength) : IPolicyProfileRule
{
    public List<string> Check(PolicyProfile item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyProfileCostCenterLengthRule(int MaxLength) : IPolicyProfileRule
{
    public List<string> Check(PolicyProfile item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy profile.</summary>
public sealed record PolicyProfileValidator(List<IPolicyProfileRule> Rules)
{
    public List<string> Validate(PolicyProfile item)
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

    public void RequireValid(PolicyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyProfile> items)
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

/// <summary>Functions of policy profile rules.</summary>
public static class PolicyProfileRules
{
    public static PolicyProfileValidator DefaultPolicyProfileValidator()
    {
        return new PolicyProfileValidator(
            new List<IPolicyProfileRule>
            {
                new PolicyProfileGraceDaysRangeRule(120, 255),
                new PolicyProfileRequiredCostCenterRule("policy profile needs this field"),
                new PolicyProfileCategoryLengthRule(3600),
                new PolicyProfileCostCenterLengthRule(3600),
            });
    }
}
