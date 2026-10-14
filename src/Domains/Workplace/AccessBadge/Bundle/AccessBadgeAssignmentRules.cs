namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>A rule a access badge assignment must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeAssignmentRule
{
    List<string> Check(AccessBadgeAssignment item);
}

public sealed record AccessBadgeAssignmentNameLengthRule(int MaxLength) : IAccessBadgeAssignmentRule
{
    public List<string> Check(AccessBadgeAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAssignmentRequiredNameRule(string Message) : IAccessBadgeAssignmentRule
{
    public List<string> Check(AccessBadgeAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAssignmentCodePrefixRule(string Prefix) : IAccessBadgeAssignmentRule
{
    public List<string> Check(AccessBadgeAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAssignmentPhoneLengthRule(int MaxLength) : IAccessBadgeAssignmentRule
{
    public List<string> Check(AccessBadgeAssignment item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAssignmentCodeLengthRule(int MaxLength) : IAccessBadgeAssignmentRule
{
    public List<string> Check(AccessBadgeAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge assignment.</summary>
public sealed record AccessBadgeAssignmentValidator(List<IAccessBadgeAssignmentRule> Rules)
{
    public List<string> Validate(AccessBadgeAssignment item)
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

    public void RequireValid(AccessBadgeAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeAssignment> items)
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

/// <summary>Functions of access badge assignment rules.</summary>
public static class AccessBadgeAssignmentRules
{
    public static AccessBadgeAssignmentValidator DefaultAccessBadgeAssignmentValidator()
    {
        return new AccessBadgeAssignmentValidator(
            new List<IAccessBadgeAssignmentRule>
            {
                new AccessBadgeAssignmentNameLengthRule(80),
                new AccessBadgeAssignmentRequiredNameRule("access badge assignment needs an entry"),
                new AccessBadgeAssignmentCodePrefixRule("X"),
                new AccessBadgeAssignmentPhoneLengthRule(1000),
                new AccessBadgeAssignmentCodeLengthRule(80),
            });
    }
}
