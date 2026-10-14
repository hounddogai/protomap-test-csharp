namespace Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>A rule a tiered laptop checkpoint must pass. Check returns its problems, or none.</summary>
public interface ITieredLaptopCheckpointRule
{
    List<string> Check(TieredLaptopCheckpoint item);
}

public sealed record TieredLaptopCheckpointApprovedGuardRule(int Limit) : ITieredLaptopCheckpointRule
{
    public List<string> Check(TieredLaptopCheckpoint item)
    {
        if (item.Approved && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopCheckpointCeilingRangeRule(double Min, double Max) : ITieredLaptopCheckpointRule
{
    public List<string> Check(TieredLaptopCheckpoint item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredLaptopCheckpointBadgeLengthRule(int MaxLength) : ITieredLaptopCheckpointRule
{
    public List<string> Check(TieredLaptopCheckpoint item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered laptop checkpoint.</summary>
public sealed record TieredLaptopCheckpointValidator(List<ITieredLaptopCheckpointRule> Rules)
{
    public List<string> Validate(TieredLaptopCheckpoint item)
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

    public void RequireValid(TieredLaptopCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLaptopCheckpoint> items)
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

/// <summary>Functions of tiered laptop checkpoint rules.</summary>
public static class TieredLaptopCheckpointRules
{
    public static TieredLaptopCheckpointValidator DefaultTieredLaptopCheckpointValidator()
    {
        return new TieredLaptopCheckpointValidator(
            new List<ITieredLaptopCheckpointRule>
            {
                new TieredLaptopCheckpointApprovedGuardRule(120),
                new TieredLaptopCheckpointCeilingRangeRule(6.3, 3855.8),
                new TieredLaptopCheckpointBadgeLengthRule(255),
            });
    }
}
