namespace Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>A rule a tiered accrual threshold must pass. Check returns its problems, or none.</summary>
public interface ITieredAccrualThresholdRule
{
    List<string> Check(TieredAccrualThreshold item);
}

public sealed record TieredAccrualThresholdRatioRangeRule(double Min, double Max) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredAccrualThresholdRequiredNameRule(string Message) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualThresholdNameLengthRule(int MaxLength) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualThresholdSegmentLengthRule(int MaxLength) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualThresholdCodeLengthRule(int MaxLength) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredAccrualThresholdRequiredSegmentRule(string Message) : ITieredAccrualThresholdRule
{
    public List<string> Check(TieredAccrualThreshold item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered accrual threshold.</summary>
public sealed record TieredAccrualThresholdValidator(List<ITieredAccrualThresholdRule> Rules)
{
    public List<string> Validate(TieredAccrualThreshold item)
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

    public void RequireValid(TieredAccrualThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredAccrualThreshold> items)
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

/// <summary>Functions of tiered accrual threshold rules.</summary>
public static class TieredAccrualThresholdRules
{
    public static TieredAccrualThresholdValidator DefaultTieredAccrualThresholdValidator()
    {
        return new TieredAccrualThresholdValidator(
            new List<ITieredAccrualThresholdRule>
            {
                new TieredAccrualThresholdRatioRangeRule(3.3, 4412.8),
                new TieredAccrualThresholdRequiredNameRule("tiered accrual threshold needs a value"),
                new TieredAccrualThresholdNameLengthRule(64),
                new TieredAccrualThresholdSegmentLengthRule(32),
                new TieredAccrualThresholdCodeLengthRule(64),
                new TieredAccrualThresholdRequiredSegmentRule("tiered accrual threshold needs an entry"),
            });
    }
}
