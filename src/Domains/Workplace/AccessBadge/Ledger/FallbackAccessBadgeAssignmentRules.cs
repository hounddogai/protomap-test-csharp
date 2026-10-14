namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>A rule a fallback access badge assignment must pass. Check returns its problems, or none.</summary>
public interface IFallbackAccessBadgeAssignmentRule
{
    List<string> Check(FallbackAccessBadgeAssignment item);
}

public sealed record FallbackAccessBadgeAssignmentCodePrefixRule(string Prefix) : IFallbackAccessBadgeAssignmentRule
{
    public List<string> Check(FallbackAccessBadgeAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAssignmentGradeLengthRule(int MaxLength) : IFallbackAccessBadgeAssignmentRule
{
    public List<string> Check(FallbackAccessBadgeAssignment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAssignmentCodeLengthRule(int MaxLength) : IFallbackAccessBadgeAssignmentRule
{
    public List<string> Check(FallbackAccessBadgeAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAccessBadgeAssignmentRequiredRegionRule(string Message) : IFallbackAccessBadgeAssignmentRule
{
    public List<string> Check(FallbackAccessBadgeAssignment item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback access badge assignment.</summary>
public sealed record FallbackAccessBadgeAssignmentValidator(List<IFallbackAccessBadgeAssignmentRule> Rules)
{
    public List<string> Validate(FallbackAccessBadgeAssignment item)
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

    public void RequireValid(FallbackAccessBadgeAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAccessBadgeAssignment> items)
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

/// <summary>Functions of fallback access badge assignment rules.</summary>
public static class FallbackAccessBadgeAssignmentRules
{
    public static FallbackAccessBadgeAssignmentValidator DefaultFallbackAccessBadgeAssignmentValidator()
    {
        return new FallbackAccessBadgeAssignmentValidator(
            new List<IFallbackAccessBadgeAssignmentRule>
            {
                new FallbackAccessBadgeAssignmentCodePrefixRule("REF"),
                new FallbackAccessBadgeAssignmentGradeLengthRule(255),
                new FallbackAccessBadgeAssignmentCodeLengthRule(64),
                new FallbackAccessBadgeAssignmentRequiredRegionRule(
                    "fallback access badge assignment needs this field"),
            });
    }
}
