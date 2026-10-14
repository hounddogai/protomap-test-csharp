namespace Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>A rule a refresh checkpoint must pass. Check returns its problems, or none.</summary>
public interface IRefreshCheckpointRule
{
    List<string> Check(RefreshCheckpoint item);
}

public sealed record RefreshCheckpointRequiredCurrencyRule(string Message) : IRefreshCheckpointRule
{
    public List<string> Check(RefreshCheckpoint item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshCheckpointCodePrefixRule(string Prefix) : IRefreshCheckpointRule
{
    public List<string> Check(RefreshCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshCheckpointTitleLengthRule(int MaxLength) : IRefreshCheckpointRule
{
    public List<string> Check(RefreshCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshCheckpointScoreRangeRule(double Min, double Max) : IRefreshCheckpointRule
{
    public List<string> Check(RefreshCheckpoint item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshCheckpointCodeLengthRule(int MaxLength) : IRefreshCheckpointRule
{
    public List<string> Check(RefreshCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh checkpoint.</summary>
public sealed record RefreshCheckpointValidator(List<IRefreshCheckpointRule> Rules)
{
    public List<string> Validate(RefreshCheckpoint item)
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

    public void RequireValid(RefreshCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshCheckpoint> items)
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

/// <summary>Functions of refresh checkpoint rules.</summary>
public static class RefreshCheckpointRules
{
    public static RefreshCheckpointValidator DefaultRefreshCheckpointValidator()
    {
        return new RefreshCheckpointValidator(
            new List<IRefreshCheckpointRule>
            {
                new RefreshCheckpointRequiredCurrencyRule("refresh checkpoint needs a value"),
                new RefreshCheckpointCodePrefixRule("X"),
                new RefreshCheckpointTitleLengthRule(1000),
                new RefreshCheckpointScoreRangeRule(9.1, 2942.4),
                new RefreshCheckpointCodeLengthRule(16),
            });
    }
}
