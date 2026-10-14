namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall checkpoint must pass. Check returns its problems, or none.</summary>
public interface ITownHallCheckpointRule
{
    List<string> Check(TownHallCheckpoint item);
}

public sealed record TownHallCheckpointAllowedTitleRule(List<string> Allowed) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCheckpointCodePrefixRule(string Prefix) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCheckpointTitleLengthRule(int MaxLength) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCheckpointFloorValueRangeRule(double Min, double Max) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallCheckpointLeaveDaysRangeRule(int Min, int Max) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallCheckpointNameLengthRule(int MaxLength) : ITownHallCheckpointRule
{
    public List<string> Check(TownHallCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall checkpoint.</summary>
public sealed record TownHallCheckpointValidator(List<ITownHallCheckpointRule> Rules)
{
    public List<string> Validate(TownHallCheckpoint item)
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

    public void RequireValid(TownHallCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallCheckpoint> items)
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

/// <summary>Functions of town hall checkpoint rules.</summary>
public static class TownHallCheckpointRules
{
    public static TownHallCheckpointValidator DefaultTownHallCheckpointValidator()
    {
        return new TownHallCheckpointValidator(
            new List<ITownHallCheckpointRule>
            {
                new TownHallCheckpointAllowedTitleRule(new List<string> { "primary", "capped", "global" }),
                new TownHallCheckpointCodePrefixRule("Q"),
                new TownHallCheckpointTitleLengthRule(3600),
                new TownHallCheckpointFloorValueRangeRule(8.9, 2743.8),
                new TownHallCheckpointLeaveDaysRangeRule(255, 86400),
                new TownHallCheckpointNameLengthRule(1000),
            });
    }
}
