namespace Acme.Workforce.Engagement.TownHall.Request;

/// <summary>A rule a legacy town hall assignment must pass. Check returns its problems, or none.</summary>
public interface ILegacyTownHallAssignmentRule
{
    List<string> Check(LegacyTownHallAssignment item);
}

public sealed record LegacyTownHallAssignmentRequiredCodeRule(string Message) : ILegacyTownHallAssignmentRule
{
    public List<string> Check(LegacyTownHallAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallAssignmentCodePrefixRule(string Prefix) : ILegacyTownHallAssignmentRule
{
    public List<string> Check(LegacyTownHallAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallAssignmentGradeLengthRule(int MaxLength) : ILegacyTownHallAssignmentRule
{
    public List<string> Check(LegacyTownHallAssignment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallAssignmentBadgeLengthRule(int MaxLength) : ILegacyTownHallAssignmentRule
{
    public List<string> Check(LegacyTownHallAssignment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTownHallAssignmentWindowRule(int MaxSpan) : ILegacyTownHallAssignmentRule
{
    public List<string> Check(LegacyTownHallAssignment item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy town hall assignment.</summary>
public sealed record LegacyTownHallAssignmentValidator(List<ILegacyTownHallAssignmentRule> Rules)
{
    public List<string> Validate(LegacyTownHallAssignment item)
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

    public void RequireValid(LegacyTownHallAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTownHallAssignment> items)
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

/// <summary>Functions of legacy town hall assignment rules.</summary>
public static class LegacyTownHallAssignmentRules
{
    public static LegacyTownHallAssignmentValidator DefaultLegacyTownHallAssignmentValidator()
    {
        return new LegacyTownHallAssignmentValidator(
            new List<ILegacyTownHallAssignmentRule>
            {
                new LegacyTownHallAssignmentRequiredCodeRule("legacy town hall assignment needs a value"),
                new LegacyTownHallAssignmentCodePrefixRule("OPS"),
                new LegacyTownHallAssignmentGradeLengthRule(3600),
                new LegacyTownHallAssignmentBadgeLengthRule(16),
                new LegacyTownHallAssignmentWindowRule(3600),
            });
    }
}
