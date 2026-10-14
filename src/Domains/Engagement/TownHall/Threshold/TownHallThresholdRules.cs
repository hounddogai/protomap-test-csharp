namespace Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>A rule a town hall threshold must pass. Check returns its problems, or none.</summary>
public interface ITownHallThresholdRule
{
    List<string> Check(TownHallThreshold item);
}

public sealed record TownHallThresholdMaximumRangeRule(double Min, double Max) : ITownHallThresholdRule
{
    public List<string> Check(TownHallThreshold item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallThresholdAmountRangeRule(double Min, double Max) : ITownHallThresholdRule
{
    public List<string> Check(TownHallThreshold item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallThresholdBudgetRangeRule(double Min, double Max) : ITownHallThresholdRule
{
    public List<string> Check(TownHallThreshold item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallThresholdVersionRangeRule(int Min, int Max) : ITownHallThresholdRule
{
    public List<string> Check(TownHallThreshold item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall threshold.</summary>
public sealed record TownHallThresholdValidator(List<ITownHallThresholdRule> Rules)
{
    public List<string> Validate(TownHallThreshold item)
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

    public void RequireValid(TownHallThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallThreshold> items)
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

/// <summary>Functions of town hall threshold rules.</summary>
public static class TownHallThresholdRules
{
    public static TownHallThresholdValidator DefaultTownHallThresholdValidator()
    {
        return new TownHallThresholdValidator(
            new List<ITownHallThresholdRule>
            {
                new TownHallThresholdMaximumRangeRule(0.5, 3895.8),
                new TownHallThresholdAmountRangeRule(2.0, 241.8),
                new TownHallThresholdBudgetRangeRule(8.7, 3335.5),
                new TownHallThresholdVersionRangeRule(64, 3600),
            });
    }
}
