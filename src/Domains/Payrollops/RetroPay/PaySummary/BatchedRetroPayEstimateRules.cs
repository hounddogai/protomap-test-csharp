namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched retro pay estimate must pass. Check returns its problems, or none.</summary>
public interface IBatchedRetroPayEstimateRule
{
    List<string> Check(BatchedRetroPayEstimate item);
}

public sealed record BatchedRetroPayEstimateMinimumRangeRule(double Min, double Max) : IBatchedRetroPayEstimateRule
{
    public List<string> Check(BatchedRetroPayEstimate item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedRetroPayEstimateTitleLengthRule(int MaxLength) : IBatchedRetroPayEstimateRule
{
    public List<string> Check(BatchedRetroPayEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRetroPayEstimateMarginRangeRule(double Min, double Max) : IBatchedRetroPayEstimateRule
{
    public List<string> Check(BatchedRetroPayEstimate item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedRetroPayEstimateAllowedCodeRule(List<string> Allowed) : IBatchedRetroPayEstimateRule
{
    public List<string> Check(BatchedRetroPayEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched retro pay estimate.</summary>
public sealed record BatchedRetroPayEstimateValidator(List<IBatchedRetroPayEstimateRule> Rules)
{
    public List<string> Validate(BatchedRetroPayEstimate item)
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

    public void RequireValid(BatchedRetroPayEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRetroPayEstimate> items)
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

/// <summary>Functions of batched retro pay estimate rules.</summary>
public static class BatchedRetroPayEstimateRules
{
    public static BatchedRetroPayEstimateValidator DefaultBatchedRetroPayEstimateValidator()
    {
        return new BatchedRetroPayEstimateValidator(
            new List<IBatchedRetroPayEstimateRule>
            {
                new BatchedRetroPayEstimateMinimumRangeRule(7.0, 2552.1),
                new BatchedRetroPayEstimateTitleLengthRule(80),
                new BatchedRetroPayEstimateMarginRangeRule(8.4, 1312.5),
                new BatchedRetroPayEstimateAllowedCodeRule(new List<string>()),
            });
    }
}
