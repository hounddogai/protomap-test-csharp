namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>A rule a merit cycle checkpoint must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleCheckpointRule
{
    List<string> Check(MeritCycleCheckpoint item);
}

public sealed record MeritCycleCheckpointRequiredLocaleRule(string Message) : IMeritCycleCheckpointRule
{
    public List<string> Check(MeritCycleCheckpoint item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleCheckpointCreatedAtRangeRule(int Min, int Max) : IMeritCycleCheckpointRule
{
    public List<string> Check(MeritCycleCheckpoint item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleCheckpointRequiredCategoryRule(string Message) : IMeritCycleCheckpointRule
{
    public List<string> Check(MeritCycleCheckpoint item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleCheckpointLimitRangeRule(int Min, int Max) : IMeritCycleCheckpointRule
{
    public List<string> Check(MeritCycleCheckpoint item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleCheckpointApprovedGuardRule(int Limit) : IMeritCycleCheckpointRule
{
    public List<string> Check(MeritCycleCheckpoint item)
    {
        if (item.Approved && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle checkpoint.</summary>
public sealed record MeritCycleCheckpointValidator(List<IMeritCycleCheckpointRule> Rules)
{
    public List<string> Validate(MeritCycleCheckpoint item)
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

    public void RequireValid(MeritCycleCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleCheckpoint> items)
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

/// <summary>Functions of merit cycle checkpoint rules.</summary>
public static class MeritCycleCheckpointRules
{
    public static MeritCycleCheckpointValidator DefaultMeritCycleCheckpointValidator()
    {
        return new MeritCycleCheckpointValidator(
            new List<IMeritCycleCheckpointRule>
            {
                new MeritCycleCheckpointRequiredLocaleRule("merit cycle checkpoint needs a value"),
                new MeritCycleCheckpointCreatedAtRangeRule(120, 1000),
                new MeritCycleCheckpointRequiredCategoryRule("merit cycle checkpoint needs an entry"),
                new MeritCycleCheckpointLimitRangeRule(32, 255),
                new MeritCycleCheckpointApprovedGuardRule(86400),
            });
    }
}
