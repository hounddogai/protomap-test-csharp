namespace Acme.Workforce.Absence.Policy.Cycle;

/// <summary>A rule a policy adjustment must pass. Check returns its problems, or none.</summary>
public interface IPolicyAdjustmentRule
{
    List<string> Check(PolicyAdjustment item);
}

public sealed record PolicyAdjustmentRequiredUnitRule(string Message) : IPolicyAdjustmentRule
{
    public List<string> Check(PolicyAdjustment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAdjustmentCodePrefixRule(string Prefix) : IPolicyAdjustmentRule
{
    public List<string> Check(PolicyAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAdjustmentAmountRangeRule(double Min, double Max) : IPolicyAdjustmentRule
{
    public List<string> Check(PolicyAdjustment item)
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

public sealed record PolicyAdjustmentSegmentLengthRule(int MaxLength) : IPolicyAdjustmentRule
{
    public List<string> Check(PolicyAdjustment item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy adjustment.</summary>
public sealed record PolicyAdjustmentValidator(List<IPolicyAdjustmentRule> Rules)
{
    public List<string> Validate(PolicyAdjustment item)
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

    public void RequireValid(PolicyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAdjustment> items)
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

/// <summary>Functions of policy adjustment rules.</summary>
public static class PolicyAdjustmentRules
{
    public static PolicyAdjustmentValidator DefaultPolicyAdjustmentValidator()
    {
        return new PolicyAdjustmentValidator(
            new List<IPolicyAdjustmentRule>
            {
                new PolicyAdjustmentRequiredUnitRule("policy adjustment needs an entry"),
                new PolicyAdjustmentCodePrefixRule("OPS"),
                new PolicyAdjustmentAmountRangeRule(6.9, 2395.0),
                new PolicyAdjustmentSegmentLengthRule(255),
            });
    }
}
