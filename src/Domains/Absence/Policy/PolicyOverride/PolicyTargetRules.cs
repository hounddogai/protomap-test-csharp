namespace Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>A rule a policy target must pass. Check returns its problems, or none.</summary>
public interface IPolicyTargetRule
{
    List<string> Check(PolicyTarget item);
}

public sealed record PolicyTargetHoursRangeRule(double Min, double Max) : IPolicyTargetRule
{
    public List<string> Check(PolicyTarget item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyTargetTitleLengthRule(int MaxLength) : IPolicyTargetRule
{
    public List<string> Check(PolicyTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyTargetUnitLengthRule(int MaxLength) : IPolicyTargetRule
{
    public List<string> Check(PolicyTarget item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyTargetRequiredCodeRule(string Message) : IPolicyTargetRule
{
    public List<string> Check(PolicyTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyTargetCategoryLengthRule(int MaxLength) : IPolicyTargetRule
{
    public List<string> Check(PolicyTarget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy target.</summary>
public sealed record PolicyTargetValidator(List<IPolicyTargetRule> Rules)
{
    public List<string> Validate(PolicyTarget item)
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

    public void RequireValid(PolicyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyTarget> items)
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

/// <summary>Functions of policy target rules.</summary>
public static class PolicyTargetRules
{
    public static PolicyTargetValidator DefaultPolicyTargetValidator()
    {
        return new PolicyTargetValidator(
            new List<IPolicyTargetRule>
            {
                new PolicyTargetHoursRangeRule(2.1, 1742.8),
                new PolicyTargetTitleLengthRule(16),
                new PolicyTargetUnitLengthRule(86400),
                new PolicyTargetRequiredCodeRule("policy target needs an entry"),
                new PolicyTargetCategoryLengthRule(16),
            });
    }
}
