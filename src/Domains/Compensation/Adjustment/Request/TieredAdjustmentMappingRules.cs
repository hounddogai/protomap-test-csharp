namespace Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>A rule a tiered adjustment mapping must pass. Check returns its problems, or none.</summary>
public interface ITieredAdjustmentMappingRule
{
    List<string> Check(TieredAdjustmentMapping item);
}

public sealed record TieredAdjustmentMappingAttemptsRangeRule(int Min, int Max) : ITieredAdjustmentMappingRule
{
    public List<string> Check(TieredAdjustmentMapping item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAdjustmentMappingCodeLengthRule(int MaxLength) : ITieredAdjustmentMappingRule
{
    public List<string> Check(TieredAdjustmentMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAdjustmentMappingBatchSizeRangeRule(int Min, int Max) : ITieredAdjustmentMappingRule
{
    public List<string> Check(TieredAdjustmentMapping item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered adjustment mapping.</summary>
public sealed record TieredAdjustmentMappingValidator(List<ITieredAdjustmentMappingRule> Rules)
{
    public List<string> Validate(TieredAdjustmentMapping item)
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

    public void RequireValid(TieredAdjustmentMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAdjustmentMapping> items)
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

/// <summary>Functions of tiered adjustment mapping rules.</summary>
public static class TieredAdjustmentMappingRules
{
    public static TieredAdjustmentMappingValidator DefaultTieredAdjustmentMappingValidator()
    {
        return new TieredAdjustmentMappingValidator(
            new List<ITieredAdjustmentMappingRule>
            {
                new TieredAdjustmentMappingAttemptsRangeRule(16, 86400),
                new TieredAdjustmentMappingCodeLengthRule(120),
                new TieredAdjustmentMappingBatchSizeRangeRule(80, 1000),
            });
    }
}
