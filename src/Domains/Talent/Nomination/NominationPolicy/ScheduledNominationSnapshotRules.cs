namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>A rule a scheduled nomination snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledNominationSnapshotRule
{
    List<string> Check(ScheduledNominationSnapshot item);
}

public sealed record ScheduledNominationSnapshotLimitRangeRule(int Min, int Max) : IScheduledNominationSnapshotRule
{
    public List<string> Check(ScheduledNominationSnapshot item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationSnapshotRequiredLocaleRule(string Message) : IScheduledNominationSnapshotRule
{
    public List<string> Check(ScheduledNominationSnapshot item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledNominationSnapshotCodePrefixRule(string Prefix) : IScheduledNominationSnapshotRule
{
    public List<string> Check(ScheduledNominationSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled nomination snapshot.</summary>
public sealed record ScheduledNominationSnapshotValidator(List<IScheduledNominationSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledNominationSnapshot item)
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

    public void RequireValid(ScheduledNominationSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledNominationSnapshot> items)
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

/// <summary>Functions of scheduled nomination snapshot rules.</summary>
public static class ScheduledNominationSnapshotRules
{
    public static ScheduledNominationSnapshotValidator DefaultScheduledNominationSnapshotValidator()
    {
        return new ScheduledNominationSnapshotValidator(
            new List<IScheduledNominationSnapshotRule>
            {
                new ScheduledNominationSnapshotLimitRangeRule(32, 64),
                new ScheduledNominationSnapshotRequiredLocaleRule("scheduled nomination snapshot needs a value"),
                new ScheduledNominationSnapshotCodePrefixRule("REF"),
            });
    }
}
