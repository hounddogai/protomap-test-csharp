namespace Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>A rule a global policy allocation must pass. Check returns its problems, or none.</summary>
public interface IGlobalPolicyAllocationRule
{
    List<string> Check(GlobalPolicyAllocation item);
}

public sealed record GlobalPolicyAllocationAmountRangeRule(double Min, double Max) : IGlobalPolicyAllocationRule
{
    public List<string> Check(GlobalPolicyAllocation item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalPolicyAllocationNameLengthRule(int MaxLength) : IGlobalPolicyAllocationRule
{
    public List<string> Check(GlobalPolicyAllocation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAllocationRequiredNameRule(string Message) : IGlobalPolicyAllocationRule
{
    public List<string> Check(GlobalPolicyAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global policy allocation.</summary>
public sealed record GlobalPolicyAllocationValidator(List<IGlobalPolicyAllocationRule> Rules)
{
    public List<string> Validate(GlobalPolicyAllocation item)
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

    public void RequireValid(GlobalPolicyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPolicyAllocation> items)
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

/// <summary>Functions of global policy allocation rules.</summary>
public static class GlobalPolicyAllocationRules
{
    public static GlobalPolicyAllocationValidator DefaultGlobalPolicyAllocationValidator()
    {
        return new GlobalPolicyAllocationValidator(
            new List<IGlobalPolicyAllocationRule>
            {
                new GlobalPolicyAllocationAmountRangeRule(1.8, 1929.2),
                new GlobalPolicyAllocationNameLengthRule(80),
                new GlobalPolicyAllocationRequiredNameRule("global policy allocation needs this field"),
            });
    }
}
