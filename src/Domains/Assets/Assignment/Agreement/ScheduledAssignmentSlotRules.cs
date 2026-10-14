namespace Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>A rule a scheduled assignment slot must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssignmentSlotRule
{
    List<string> Check(ScheduledAssignmentSlot item);
}

public sealed record ScheduledAssignmentSlotCodePrefixRule(string Prefix) : IScheduledAssignmentSlotRule
{
    public List<string> Check(ScheduledAssignmentSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentSlotTitleLengthRule(int MaxLength) : IScheduledAssignmentSlotRule
{
    public List<string> Check(ScheduledAssignmentSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssignmentSlotStartsAtRangeRule(int Min, int Max) : IScheduledAssignmentSlotRule
{
    public List<string> Check(ScheduledAssignmentSlot item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled assignment slot.</summary>
public sealed record ScheduledAssignmentSlotValidator(List<IScheduledAssignmentSlotRule> Rules)
{
    public List<string> Validate(ScheduledAssignmentSlot item)
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

    public void RequireValid(ScheduledAssignmentSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssignmentSlot> items)
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

/// <summary>Functions of scheduled assignment slot rules.</summary>
public static class ScheduledAssignmentSlotRules
{
    public static ScheduledAssignmentSlotValidator DefaultScheduledAssignmentSlotValidator()
    {
        return new ScheduledAssignmentSlotValidator(
            new List<IScheduledAssignmentSlotRule>
            {
                new ScheduledAssignmentSlotCodePrefixRule("REF"),
                new ScheduledAssignmentSlotTitleLengthRule(255),
                new ScheduledAssignmentSlotStartsAtRangeRule(16, 255),
            });
    }
}
