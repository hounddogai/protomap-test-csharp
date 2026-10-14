namespace Acme.Workforce.Absence.Policy.Notice;

/// <summary>A rule a regional policy assignment must pass. Check returns its problems, or none.</summary>
public interface IRegionalPolicyAssignmentRule
{
    List<string> Check(RegionalPolicyAssignment item);
}

public sealed record RegionalPolicyAssignmentCodePrefixRule(string Prefix) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyAssignmentRequiredUnitRule(string Message) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyAssignmentRequiredTitleRule(string Message) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyAssignmentTitleLengthRule(int MaxLength) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyAssignmentCodeLengthRule(int MaxLength) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyAssignmentUnitLengthRule(int MaxLength) : IRegionalPolicyAssignmentRule
{
    public List<string> Check(RegionalPolicyAssignment item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional policy assignment.</summary>
public sealed record RegionalPolicyAssignmentValidator(List<IRegionalPolicyAssignmentRule> Rules)
{
    public List<string> Validate(RegionalPolicyAssignment item)
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

    public void RequireValid(RegionalPolicyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPolicyAssignment> items)
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

/// <summary>Functions of regional policy assignment rules.</summary>
public static class RegionalPolicyAssignmentRules
{
    public static RegionalPolicyAssignmentValidator DefaultRegionalPolicyAssignmentValidator()
    {
        return new RegionalPolicyAssignmentValidator(
            new List<IRegionalPolicyAssignmentRule>
            {
                new RegionalPolicyAssignmentCodePrefixRule("OPS"),
                new RegionalPolicyAssignmentRequiredUnitRule("regional policy assignment needs a value"),
                new RegionalPolicyAssignmentRequiredTitleRule("regional policy assignment needs a value"),
                new RegionalPolicyAssignmentTitleLengthRule(80),
                new RegionalPolicyAssignmentCodeLengthRule(255),
                new RegionalPolicyAssignmentUnitLengthRule(32),
            });
    }
}
