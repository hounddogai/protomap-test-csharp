namespace Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>A rule a scheduled offboarding rollup must pass. Check returns its problems, or none.</summary>
public interface IScheduledOffboardingRollupRule
{
    List<string> Check(ScheduledOffboardingRollup item);
}

public sealed record ScheduledOffboardingRollupLocaleLengthRule(int MaxLength) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingRollupRequiredTeamRule(string Message) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingRollupRequiredLocaleRule(string Message) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingRollupEndsAtRangeRule(int Min, int Max) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledOffboardingRollupCodePrefixRule(string Prefix) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledOffboardingRollupTeamLengthRule(int MaxLength) : IScheduledOffboardingRollupRule
{
    public List<string> Check(ScheduledOffboardingRollup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled offboarding rollup.</summary>
public sealed record ScheduledOffboardingRollupValidator(List<IScheduledOffboardingRollupRule> Rules)
{
    public List<string> Validate(ScheduledOffboardingRollup item)
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

    public void RequireValid(ScheduledOffboardingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledOffboardingRollup> items)
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

/// <summary>Functions of scheduled offboarding rollup rules.</summary>
public static class ScheduledOffboardingRollupRules
{
    public static ScheduledOffboardingRollupValidator DefaultScheduledOffboardingRollupValidator()
    {
        return new ScheduledOffboardingRollupValidator(
            new List<IScheduledOffboardingRollupRule>
            {
                new ScheduledOffboardingRollupLocaleLengthRule(1000),
                new ScheduledOffboardingRollupRequiredTeamRule("scheduled offboarding rollup needs an entry"),
                new ScheduledOffboardingRollupRequiredLocaleRule("scheduled offboarding rollup needs this field"),
                new ScheduledOffboardingRollupEndsAtRangeRule(80, 80),
                new ScheduledOffboardingRollupCodePrefixRule("X"),
                new ScheduledOffboardingRollupTeamLengthRule(16),
            });
    }
}
