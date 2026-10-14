namespace Acme.Workforce.Absence.Policy.Baseline;

/// <summary>A rule a policy baseline must pass. Check returns its problems, or none.</summary>
public interface IPolicyBaselineRule
{
    List<string> Check(PolicyBaseline item);
}

public sealed record PolicyBaselineFloorValueRangeRule(double Min, double Max) : IPolicyBaselineRule
{
    public List<string> Check(PolicyBaseline item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyBaselineRequiredCodeRule(string Message) : IPolicyBaselineRule
{
    public List<string> Check(PolicyBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyBaselineCodeLengthRule(int MaxLength) : IPolicyBaselineRule
{
    public List<string> Check(PolicyBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy baseline.</summary>
public sealed record PolicyBaselineValidator(List<IPolicyBaselineRule> Rules)
{
    public List<string> Validate(PolicyBaseline item)
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

    public void RequireValid(PolicyBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyBaseline> items)
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

/// <summary>Functions of policy baseline rules.</summary>
public static class PolicyBaselineRules
{
    public static PolicyBaselineValidator DefaultPolicyBaselineValidator()
    {
        return new PolicyBaselineValidator(
            new List<IPolicyBaselineRule>
            {
                new PolicyBaselineFloorValueRangeRule(9.4, 4628.0),
                new PolicyBaselineRequiredCodeRule("policy baseline needs this field"),
                new PolicyBaselineCodeLengthRule(32),
            });
    }
}
