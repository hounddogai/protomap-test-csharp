namespace Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>A rule a tiered merit cycle checkpoint must pass. Check returns its problems, or none.</summary>
public interface ITieredMeritCycleCheckpointRule
{
    List<string> Check(TieredMeritCycleCheckpoint item);
}

public sealed record TieredMeritCycleCheckpointCodeLengthRule(int MaxLength) : ITieredMeritCycleCheckpointRule
{
    public List<string> Check(TieredMeritCycleCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMeritCycleCheckpointCeilingRangeRule(double Min, double Max) : ITieredMeritCycleCheckpointRule
{
    public List<string> Check(TieredMeritCycleCheckpoint item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMeritCycleCheckpointRequiredTitleRule(string Message) : ITieredMeritCycleCheckpointRule
{
    public List<string> Check(TieredMeritCycleCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredMeritCycleCheckpointExpiresAtRangeRule(int Min, int Max) : ITieredMeritCycleCheckpointRule
{
    public List<string> Check(TieredMeritCycleCheckpoint item)
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

public sealed record TieredMeritCycleCheckpointAmountRangeRule(double Min, double Max) : ITieredMeritCycleCheckpointRule
{
    public List<string> Check(TieredMeritCycleCheckpoint item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered merit cycle checkpoint.</summary>
public sealed record TieredMeritCycleCheckpointValidator(List<ITieredMeritCycleCheckpointRule> Rules)
{
    public List<string> Validate(TieredMeritCycleCheckpoint item)
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

    public void RequireValid(TieredMeritCycleCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredMeritCycleCheckpoint> items)
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

/// <summary>Functions of tiered merit cycle checkpoint rules.</summary>
public static class TieredMeritCycleCheckpointRules
{
    public static TieredMeritCycleCheckpointValidator DefaultTieredMeritCycleCheckpointValidator()
    {
        return new TieredMeritCycleCheckpointValidator(
            new List<ITieredMeritCycleCheckpointRule>
            {
                new TieredMeritCycleCheckpointCodeLengthRule(80),
                new TieredMeritCycleCheckpointCeilingRangeRule(3.5, 3050.8),
                new TieredMeritCycleCheckpointRequiredTitleRule("tiered merit cycle checkpoint needs a value"),
                new TieredMeritCycleCheckpointExpiresAtRangeRule(255, 86400),
                new TieredMeritCycleCheckpointAmountRangeRule(2.7, 3183.3),
            });
    }
}
