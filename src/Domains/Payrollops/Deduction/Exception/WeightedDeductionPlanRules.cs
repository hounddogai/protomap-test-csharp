namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>A rule a weighted deduction plan must pass. Check returns its problems, or none.</summary>
public interface IWeightedDeductionPlanRule
{
    List<string> Check(WeightedDeductionPlan item);
}

public sealed record WeightedDeductionPlanThresholdRangeRule(double Min, double Max) : IWeightedDeductionPlanRule
{
    public List<string> Check(WeightedDeductionPlan item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeductionPlanCodeLengthRule(int MaxLength) : IWeightedDeductionPlanRule
{
    public List<string> Check(WeightedDeductionPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeductionPlanVersionRangeRule(int Min, int Max) : IWeightedDeductionPlanRule
{
    public List<string> Check(WeightedDeductionPlan item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted deduction plan.</summary>
public sealed record WeightedDeductionPlanValidator(List<IWeightedDeductionPlanRule> Rules)
{
    public List<string> Validate(WeightedDeductionPlan item)
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

    public void RequireValid(WeightedDeductionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedDeductionPlan> items)
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

/// <summary>Functions of weighted deduction plan rules.</summary>
public static class WeightedDeductionPlanRules
{
    public static WeightedDeductionPlanValidator DefaultWeightedDeductionPlanValidator()
    {
        return new WeightedDeductionPlanValidator(
            new List<IWeightedDeductionPlanRule>
            {
                new WeightedDeductionPlanThresholdRangeRule(9.1, 3327.2),
                new WeightedDeductionPlanCodeLengthRule(3600),
                new WeightedDeductionPlanVersionRangeRule(16, 1000),
            });
    }
}
