namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk segment must pass. Check returns its problems, or none.</summary>
public interface IDeskSegmentRule
{
    List<string> Check(DeskSegment item);
}

public sealed record DeskSegmentRequiredTitleRule(string Message) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSegmentRequiredCodeRule(string Message) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSegmentCodePrefixRule(string Prefix) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskSegmentRequiredUnitRule(string Message) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSegmentManagerIdLengthRule(int MaxLength) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskSegmentAllowedCodeRule(List<string> Allowed) : IDeskSegmentRule
{
    public List<string> Check(DeskSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk segment.</summary>
public sealed record DeskSegmentValidator(List<IDeskSegmentRule> Rules)
{
    public List<string> Validate(DeskSegment item)
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

    public void RequireValid(DeskSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskSegment> items)
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

/// <summary>Functions of desk segment rules.</summary>
public static class DeskSegmentRules
{
    public static DeskSegmentValidator DefaultDeskSegmentValidator()
    {
        return new DeskSegmentValidator(
            new List<IDeskSegmentRule>
            {
                new DeskSegmentRequiredTitleRule("desk segment needs an entry"),
                new DeskSegmentRequiredCodeRule("desk segment needs a value"),
                new DeskSegmentCodePrefixRule("REF"),
                new DeskSegmentRequiredUnitRule("desk segment needs an entry"),
                new DeskSegmentManagerIdLengthRule(80),
                new DeskSegmentAllowedCodeRule(new List<string> { "lenient" }),
            });
    }
}
