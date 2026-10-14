namespace Acme.Workforce.Absence.Balance.Summary;

/// <summary>A rule a weighted balance summary must pass. Check returns its problems, or none.</summary>
public interface IWeightedBalanceSummaryRule
{
    List<string> Check(WeightedBalanceSummary item);
}

public sealed record WeightedBalanceSummaryThresholdRangeRule(double Min, double Max) : IWeightedBalanceSummaryRule
{
    public List<string> Check(WeightedBalanceSummary item)
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

public sealed record WeightedBalanceSummaryDescriptionLengthRule(int MaxLength) : IWeightedBalanceSummaryRule
{
    public List<string> Check(WeightedBalanceSummary item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceSummaryRequiredCodeRule(string Message) : IWeightedBalanceSummaryRule
{
    public List<string> Check(WeightedBalanceSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted balance summary.</summary>
public sealed record WeightedBalanceSummaryValidator(List<IWeightedBalanceSummaryRule> Rules)
{
    public List<string> Validate(WeightedBalanceSummary item)
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

    public void RequireValid(WeightedBalanceSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBalanceSummary> items)
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

/// <summary>Functions of weighted balance summary rules.</summary>
public static class WeightedBalanceSummaryRules
{
    public static WeightedBalanceSummaryValidator DefaultWeightedBalanceSummaryValidator()
    {
        return new WeightedBalanceSummaryValidator(
            new List<IWeightedBalanceSummaryRule>
            {
                new WeightedBalanceSummaryThresholdRangeRule(2.4, 2925.4),
                new WeightedBalanceSummaryDescriptionLengthRule(255),
                new WeightedBalanceSummaryRequiredCodeRule("weighted balance summary needs this field"),
            });
    }
}
