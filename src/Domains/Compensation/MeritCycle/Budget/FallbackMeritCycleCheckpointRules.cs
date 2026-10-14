namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback merit cycle checkpoint must pass. Check returns its problems, or none.</summary>
public interface IFallbackMeritCycleCheckpointRule
{
    List<string> Check(FallbackMeritCycleCheckpoint item);
}

public sealed record FallbackMeritCycleCheckpointAllowedCodeRule(List<string> Allowed) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleCheckpointNameLengthRule(int MaxLength) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleCheckpointLeaveDaysRangeRule(int Min, int Max) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleCheckpointApprovedGuardRule(double Limit) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleCheckpointRequiredNameRule(string Message) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackMeritCycleCheckpointLimitRangeRule(int Min, int Max) : IFallbackMeritCycleCheckpointRule
{
    public List<string> Check(FallbackMeritCycleCheckpoint item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback merit cycle checkpoint.</summary>
public sealed record FallbackMeritCycleCheckpointValidator(List<IFallbackMeritCycleCheckpointRule> Rules)
{
    public List<string> Validate(FallbackMeritCycleCheckpoint item)
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

    public void RequireValid(FallbackMeritCycleCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackMeritCycleCheckpoint> items)
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

/// <summary>Functions of fallback merit cycle checkpoint rules.</summary>
public static class FallbackMeritCycleCheckpointRules
{
    public static FallbackMeritCycleCheckpointValidator DefaultFallbackMeritCycleCheckpointValidator()
    {
        return new FallbackMeritCycleCheckpointValidator(
            new List<IFallbackMeritCycleCheckpointRule>
            {
                new FallbackMeritCycleCheckpointAllowedCodeRule(new List<string> { "adaptive", "fallback" }),
                new FallbackMeritCycleCheckpointNameLengthRule(120),
                new FallbackMeritCycleCheckpointLeaveDaysRangeRule(32, 32),
                new FallbackMeritCycleCheckpointApprovedGuardRule(2.6),
                new FallbackMeritCycleCheckpointRequiredNameRule("fallback merit cycle checkpoint needs an entry"),
                new FallbackMeritCycleCheckpointLimitRangeRule(16, 86400),
            });
    }
}
