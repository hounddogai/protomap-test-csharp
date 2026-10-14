namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>A rule a succession plan target must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanTargetRule
{
    List<string> Check(SuccessionPlanTarget item);
}

public sealed record SuccessionPlanTargetCodePrefixRule(string Prefix) : ISuccessionPlanTargetRule
{
    public List<string> Check(SuccessionPlanTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanTargetLimitRangeRule(int Min, int Max) : ISuccessionPlanTargetRule
{
    public List<string> Check(SuccessionPlanTarget item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanTargetNameLengthRule(int MaxLength) : ISuccessionPlanTargetRule
{
    public List<string> Check(SuccessionPlanTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanTargetThresholdRangeRule(double Min, double Max) : ISuccessionPlanTargetRule
{
    public List<string> Check(SuccessionPlanTarget item)
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

public sealed record SuccessionPlanTargetCostCenterLengthRule(int MaxLength) : ISuccessionPlanTargetRule
{
    public List<string> Check(SuccessionPlanTarget item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan target.</summary>
public sealed record SuccessionPlanTargetValidator(List<ISuccessionPlanTargetRule> Rules)
{
    public List<string> Validate(SuccessionPlanTarget item)
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

    public void RequireValid(SuccessionPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanTarget> items)
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

/// <summary>Functions of succession plan target rules.</summary>
public static class SuccessionPlanTargetRules
{
    public static SuccessionPlanTargetValidator DefaultSuccessionPlanTargetValidator()
    {
        return new SuccessionPlanTargetValidator(
            new List<ISuccessionPlanTargetRule>
            {
                new SuccessionPlanTargetCodePrefixRule("OPS"),
                new SuccessionPlanTargetLimitRangeRule(16, 120),
                new SuccessionPlanTargetNameLengthRule(32),
                new SuccessionPlanTargetThresholdRangeRule(8.1, 4310.1),
                new SuccessionPlanTargetCostCenterLengthRule(16),
            });
    }
}
