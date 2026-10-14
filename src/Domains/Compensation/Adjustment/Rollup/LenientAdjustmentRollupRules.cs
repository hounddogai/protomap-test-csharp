namespace Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>A rule a lenient adjustment rollup must pass. Check returns its problems, or none.</summary>
public interface ILenientAdjustmentRollupRule
{
    List<string> Check(LenientAdjustmentRollup item);
}

public sealed record LenientAdjustmentRollupTitleLengthRule(int MaxLength) : ILenientAdjustmentRollupRule
{
    public List<string> Check(LenientAdjustmentRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAdjustmentRollupRequiredTitleRule(string Message) : ILenientAdjustmentRollupRule
{
    public List<string> Check(LenientAdjustmentRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAdjustmentRollupRateRangeRule(double Min, double Max) : ILenientAdjustmentRollupRule
{
    public List<string> Check(LenientAdjustmentRollup item)
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

/// <summary>Runs every rule of a lenient adjustment rollup.</summary>
public sealed record LenientAdjustmentRollupValidator(List<ILenientAdjustmentRollupRule> Rules)
{
    public List<string> Validate(LenientAdjustmentRollup item)
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

    public void RequireValid(LenientAdjustmentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAdjustmentRollup> items)
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

/// <summary>Functions of lenient adjustment rollup rules.</summary>
public static class LenientAdjustmentRollupRules
{
    public static LenientAdjustmentRollupValidator DefaultLenientAdjustmentRollupValidator()
    {
        return new LenientAdjustmentRollupValidator(
            new List<ILenientAdjustmentRollupRule>
            {
                new LenientAdjustmentRollupTitleLengthRule(16),
                new LenientAdjustmentRollupRequiredTitleRule("lenient adjustment rollup needs a value"),
                new LenientAdjustmentRollupRateRangeRule(7.5, 1244.7),
            });
    }
}
