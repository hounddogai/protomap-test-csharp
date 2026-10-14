namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>A rule a rolling merit cycle target must pass. Check returns its problems, or none.</summary>
public interface IRollingMeritCycleTargetRule
{
    List<string> Check(RollingMeritCycleTarget item);
}

public sealed record RollingMeritCycleTargetApprovedGuardRule(int Limit) : IRollingMeritCycleTargetRule
{
    public List<string> Check(RollingMeritCycleTarget item)
    {
        if (item.Approved && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleTargetCodePrefixRule(string Prefix) : IRollingMeritCycleTargetRule
{
    public List<string> Check(RollingMeritCycleTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleTargetRequiredCodeRule(string Message) : IRollingMeritCycleTargetRule
{
    public List<string> Check(RollingMeritCycleTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMeritCycleTargetTitleLengthRule(int MaxLength) : IRollingMeritCycleTargetRule
{
    public List<string> Check(RollingMeritCycleTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling merit cycle target.</summary>
public sealed record RollingMeritCycleTargetValidator(List<IRollingMeritCycleTargetRule> Rules)
{
    public List<string> Validate(RollingMeritCycleTarget item)
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

    public void RequireValid(RollingMeritCycleTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMeritCycleTarget> items)
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

/// <summary>Functions of rolling merit cycle target rules.</summary>
public static class RollingMeritCycleTargetRules
{
    public static RollingMeritCycleTargetValidator DefaultRollingMeritCycleTargetValidator()
    {
        return new RollingMeritCycleTargetValidator(
            new List<IRollingMeritCycleTargetRule>
            {
                new RollingMeritCycleTargetApprovedGuardRule(32),
                new RollingMeritCycleTargetCodePrefixRule("X"),
                new RollingMeritCycleTargetRequiredCodeRule("rolling merit cycle target needs this field"),
                new RollingMeritCycleTargetTitleLengthRule(64),
            });
    }
}
