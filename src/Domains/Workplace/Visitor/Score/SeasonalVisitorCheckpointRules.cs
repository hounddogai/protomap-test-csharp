namespace Acme.Workforce.Workplace.Visitor.Score;

/// <summary>A rule a seasonal visitor checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISeasonalVisitorCheckpointRule
{
    List<string> Check(SeasonalVisitorCheckpoint item);
}

public sealed record SeasonalVisitorCheckpointRequiredLocaleRule(string Message) : ISeasonalVisitorCheckpointRule
{
    public List<string> Check(SeasonalVisitorCheckpoint item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalVisitorCheckpointPinnedGuardRule(int Limit) : ISeasonalVisitorCheckpointRule
{
    public List<string> Check(SeasonalVisitorCheckpoint item)
    {
        if (item.Pinned && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalVisitorCheckpointCodeLengthRule(int MaxLength) : ISeasonalVisitorCheckpointRule
{
    public List<string> Check(SeasonalVisitorCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalVisitorCheckpointManagerIdLengthRule(int MaxLength) : ISeasonalVisitorCheckpointRule
{
    public List<string> Check(SeasonalVisitorCheckpoint item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal visitor checkpoint.</summary>
public sealed record SeasonalVisitorCheckpointValidator(List<ISeasonalVisitorCheckpointRule> Rules)
{
    public List<string> Validate(SeasonalVisitorCheckpoint item)
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

    public void RequireValid(SeasonalVisitorCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalVisitorCheckpoint> items)
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

/// <summary>Functions of seasonal visitor checkpoint rules.</summary>
public static class SeasonalVisitorCheckpointRules
{
    public static SeasonalVisitorCheckpointValidator DefaultSeasonalVisitorCheckpointValidator()
    {
        return new SeasonalVisitorCheckpointValidator(
            new List<ISeasonalVisitorCheckpointRule>
            {
                new SeasonalVisitorCheckpointRequiredLocaleRule("seasonal visitor checkpoint needs an entry"),
                new SeasonalVisitorCheckpointPinnedGuardRule(255),
                new SeasonalVisitorCheckpointCodeLengthRule(120),
                new SeasonalVisitorCheckpointManagerIdLengthRule(1000),
            });
    }
}
