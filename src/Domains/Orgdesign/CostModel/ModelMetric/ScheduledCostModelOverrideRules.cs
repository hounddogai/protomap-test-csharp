namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>A rule a scheduled cost model override must pass. Check returns its problems, or none.</summary>
public interface IScheduledCostModelOverrideRule
{
    List<string> Check(ScheduledCostModelOverride item);
}

public sealed record ScheduledCostModelOverridePinnedGuardRule(double Limit) : IScheduledCostModelOverrideRule
{
    public List<string> Check(ScheduledCostModelOverride item)
    {
        if (item.Pinned && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelOverrideCodeLengthRule(int MaxLength) : IScheduledCostModelOverrideRule
{
    public List<string> Check(ScheduledCostModelOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelOverrideRequiredSummaryRule(string Message) : IScheduledCostModelOverrideRule
{
    public List<string> Check(ScheduledCostModelOverride item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCostModelOverrideRateRangeRule(double Min, double Max) : IScheduledCostModelOverrideRule
{
    public List<string> Check(ScheduledCostModelOverride item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled cost model override.</summary>
public sealed record ScheduledCostModelOverrideValidator(List<IScheduledCostModelOverrideRule> Rules)
{
    public List<string> Validate(ScheduledCostModelOverride item)
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

    public void RequireValid(ScheduledCostModelOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledCostModelOverride> items)
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

/// <summary>Functions of scheduled cost model override rules.</summary>
public static class ScheduledCostModelOverrideRules
{
    public static ScheduledCostModelOverrideValidator DefaultScheduledCostModelOverrideValidator()
    {
        return new ScheduledCostModelOverrideValidator(
            new List<IScheduledCostModelOverrideRule>
            {
                new ScheduledCostModelOverridePinnedGuardRule(8.9),
                new ScheduledCostModelOverrideCodeLengthRule(3600),
                new ScheduledCostModelOverrideRequiredSummaryRule("scheduled cost model override needs a value"),
                new ScheduledCostModelOverrideRateRangeRule(5.8, 3541.1),
            });
    }
}
