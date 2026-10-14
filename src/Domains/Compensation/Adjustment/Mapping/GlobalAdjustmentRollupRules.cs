namespace Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>A rule a global adjustment rollup must pass. Check returns its problems, or none.</summary>
public interface IGlobalAdjustmentRollupRule
{
    List<string> Check(GlobalAdjustmentRollup item);
}

public sealed record GlobalAdjustmentRollupRequiredSalaryBandRule(string Message) : IGlobalAdjustmentRollupRule
{
    public List<string> Check(GlobalAdjustmentRollup item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentRollupThresholdRangeRule(double Min, double Max) : IGlobalAdjustmentRollupRule
{
    public List<string> Check(GlobalAdjustmentRollup item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAdjustmentRollupAmountRangeRule(double Min, double Max) : IGlobalAdjustmentRollupRule
{
    public List<string> Check(GlobalAdjustmentRollup item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global adjustment rollup.</summary>
public sealed record GlobalAdjustmentRollupValidator(List<IGlobalAdjustmentRollupRule> Rules)
{
    public List<string> Validate(GlobalAdjustmentRollup item)
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

    public void RequireValid(GlobalAdjustmentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAdjustmentRollup> items)
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

/// <summary>Functions of global adjustment rollup rules.</summary>
public static class GlobalAdjustmentRollupRules
{
    public static GlobalAdjustmentRollupValidator DefaultGlobalAdjustmentRollupValidator()
    {
        return new GlobalAdjustmentRollupValidator(
            new List<IGlobalAdjustmentRollupRule>
            {
                new GlobalAdjustmentRollupRequiredSalaryBandRule("global adjustment rollup needs this field"),
                new GlobalAdjustmentRollupThresholdRangeRule(0.7, 2503.9),
                new GlobalAdjustmentRollupAmountRangeRule(8.4, 3976.2),
            });
    }
}
