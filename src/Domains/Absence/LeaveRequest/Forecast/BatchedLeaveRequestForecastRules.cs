namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>A rule a batched leave request forecast must pass. Check returns its problems, or none.</summary>
public interface IBatchedLeaveRequestForecastRule
{
    List<string> Check(BatchedLeaveRequestForecast item);
}

public sealed record BatchedLeaveRequestForecastNameLengthRule(int MaxLength) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestForecastDiscountRangeRule(double Min, double Max) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedLeaveRequestForecastCodeLengthRule(int MaxLength) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestForecastUpdatedAtRangeRule(int Min, int Max) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestForecastBatchSizeRangeRule(int Min, int Max) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestForecastTitleLengthRule(int MaxLength) : IBatchedLeaveRequestForecastRule
{
    public List<string> Check(BatchedLeaveRequestForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched leave request forecast.</summary>
public sealed record BatchedLeaveRequestForecastValidator(List<IBatchedLeaveRequestForecastRule> Rules)
{
    public List<string> Validate(BatchedLeaveRequestForecast item)
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

    public void RequireValid(BatchedLeaveRequestForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLeaveRequestForecast> items)
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

/// <summary>Functions of batched leave request forecast rules.</summary>
public static class BatchedLeaveRequestForecastRules
{
    public static BatchedLeaveRequestForecastValidator DefaultBatchedLeaveRequestForecastValidator()
    {
        return new BatchedLeaveRequestForecastValidator(
            new List<IBatchedLeaveRequestForecastRule>
            {
                new BatchedLeaveRequestForecastNameLengthRule(120),
                new BatchedLeaveRequestForecastDiscountRangeRule(1.2, 1106.1),
                new BatchedLeaveRequestForecastCodeLengthRule(16),
                new BatchedLeaveRequestForecastUpdatedAtRangeRule(32, 1000),
                new BatchedLeaveRequestForecastBatchSizeRangeRule(80, 120),
                new BatchedLeaveRequestForecastTitleLengthRule(3600),
            });
    }
}
