namespace Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>A rule a access badge segment must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeSegmentRule
{
    List<string> Check(AccessBadgeSegment item);
}

public sealed record AccessBadgeSegmentNameLengthRule(int MaxLength) : IAccessBadgeSegmentRule
{
    public List<string> Check(AccessBadgeSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSegmentRequiredChecksumRule(string Message) : IAccessBadgeSegmentRule
{
    public List<string> Check(AccessBadgeSegment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSegmentExpiresAtRangeRule(int Min, int Max) : IAccessBadgeSegmentRule
{
    public List<string> Check(AccessBadgeSegment item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeSegmentCodeLengthRule(int MaxLength) : IAccessBadgeSegmentRule
{
    public List<string> Check(AccessBadgeSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSegmentRequiredSourceRule(string Message) : IAccessBadgeSegmentRule
{
    public List<string> Check(AccessBadgeSegment item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge segment.</summary>
public sealed record AccessBadgeSegmentValidator(List<IAccessBadgeSegmentRule> Rules)
{
    public List<string> Validate(AccessBadgeSegment item)
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

    public void RequireValid(AccessBadgeSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeSegment> items)
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

/// <summary>Functions of access badge segment rules.</summary>
public static class AccessBadgeSegmentRules
{
    public static AccessBadgeSegmentValidator DefaultAccessBadgeSegmentValidator()
    {
        return new AccessBadgeSegmentValidator(
            new List<IAccessBadgeSegmentRule>
            {
                new AccessBadgeSegmentNameLengthRule(120),
                new AccessBadgeSegmentRequiredChecksumRule("access badge segment needs an entry"),
                new AccessBadgeSegmentExpiresAtRangeRule(80, 255),
                new AccessBadgeSegmentCodeLengthRule(120),
                new AccessBadgeSegmentRequiredSourceRule("access badge segment needs a value"),
            });
    }
}
