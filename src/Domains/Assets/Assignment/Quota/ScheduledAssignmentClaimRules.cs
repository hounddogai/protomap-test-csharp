namespace Acme.Workforce.Assets.Assignment.Quota;

/// <summary>A rule a scheduled assignment claim must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssignmentClaimRule
{
    List<string> Check(ScheduledAssignmentClaim item);
}

public sealed record ScheduledAssignmentClaimCodePrefixRule(string Prefix) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentClaimHoursRangeRule(double Min, double Max) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentClaimTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ScheduledAssignmentClaimTitleLengthRule(int MaxLength) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentClaimVisibleGuardRule(double Limit) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        if (item.Visible && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentClaimChannelLengthRule(int MaxLength) : IScheduledAssignmentClaimRule
{
    public List<string> Check(ScheduledAssignmentClaim item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled assignment claim.</summary>
public sealed record ScheduledAssignmentClaimValidator(List<IScheduledAssignmentClaimRule> Rules)
{
    public List<string> Validate(ScheduledAssignmentClaim item)
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

    public void RequireValid(ScheduledAssignmentClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssignmentClaim> items)
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

/// <summary>Functions of scheduled assignment claim rules.</summary>
public static class ScheduledAssignmentClaimRules
{
    public static ScheduledAssignmentClaimValidator DefaultScheduledAssignmentClaimValidator()
    {
        return new ScheduledAssignmentClaimValidator(
            new List<IScheduledAssignmentClaimRule>
            {
                new ScheduledAssignmentClaimCodePrefixRule("X"),
                new ScheduledAssignmentClaimHoursRangeRule(2.3, 659.4),
                new ScheduledAssignmentClaimTagsLimitRule(255, new List<string> { "legacy", "fallback", "fallback" }),
                new ScheduledAssignmentClaimTitleLengthRule(1000),
                new ScheduledAssignmentClaimVisibleGuardRule(5.3),
                new ScheduledAssignmentClaimChannelLengthRule(64),
            });
    }
}
