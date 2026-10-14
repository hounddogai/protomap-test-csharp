namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>A rule a weighted merit cycle bundle must pass. Check returns its problems, or none.</summary>
public interface IWeightedMeritCycleBundleRule
{
    List<string> Check(WeightedMeritCycleBundle item);
}

public sealed record WeightedMeritCycleBundleTenureMonthsRangeRule(int Min, int Max) : IWeightedMeritCycleBundleRule
{
    public List<string> Check(WeightedMeritCycleBundle item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedMeritCycleBundleBudgetRangeRule(double Min, double Max) : IWeightedMeritCycleBundleRule
{
    public List<string> Check(WeightedMeritCycleBundle item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedMeritCycleBundleRetriesRangeRule(int Min, int Max) : IWeightedMeritCycleBundleRule
{
    public List<string> Check(WeightedMeritCycleBundle item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted merit cycle bundle.</summary>
public sealed record WeightedMeritCycleBundleValidator(List<IWeightedMeritCycleBundleRule> Rules)
{
    public List<string> Validate(WeightedMeritCycleBundle item)
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

    public void RequireValid(WeightedMeritCycleBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedMeritCycleBundle> items)
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

/// <summary>Functions of weighted merit cycle bundle rules.</summary>
public static class WeightedMeritCycleBundleRules
{
    public static WeightedMeritCycleBundleValidator DefaultWeightedMeritCycleBundleValidator()
    {
        return new WeightedMeritCycleBundleValidator(
            new List<IWeightedMeritCycleBundleRule>
            {
                new WeightedMeritCycleBundleTenureMonthsRangeRule(64, 120),
                new WeightedMeritCycleBundleBudgetRangeRule(9.3, 2818.7),
                new WeightedMeritCycleBundleRetriesRangeRule(16, 3600),
            });
    }
}
