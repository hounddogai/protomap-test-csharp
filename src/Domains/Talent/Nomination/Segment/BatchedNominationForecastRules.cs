namespace Acme.Workforce.Talent.Nomination.Segment;

/// <summary>A rule a batched nomination forecast must pass. Check returns its problems, or none.</summary>
public interface IBatchedNominationForecastRule
{
    List<string> Check(BatchedNominationForecast item);
}

public sealed record BatchedNominationForecastLeaveDaysRangeRule(int Min, int Max) : IBatchedNominationForecastRule
{
    public List<string> Check(BatchedNominationForecast item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedNominationForecastOwnerIdLengthRule(int MaxLength) : IBatchedNominationForecastRule
{
    public List<string> Check(BatchedNominationForecast item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedNominationForecastCodeLengthRule(int MaxLength) : IBatchedNominationForecastRule
{
    public List<string> Check(BatchedNominationForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched nomination forecast.</summary>
public sealed record BatchedNominationForecastValidator(List<IBatchedNominationForecastRule> Rules)
{
    public List<string> Validate(BatchedNominationForecast item)
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

    public void RequireValid(BatchedNominationForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedNominationForecast> items)
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

/// <summary>Functions of batched nomination forecast rules.</summary>
public static class BatchedNominationForecastRules
{
    public static BatchedNominationForecastValidator DefaultBatchedNominationForecastValidator()
    {
        return new BatchedNominationForecastValidator(
            new List<IBatchedNominationForecastRule>
            {
                new BatchedNominationForecastLeaveDaysRangeRule(80, 3600),
                new BatchedNominationForecastOwnerIdLengthRule(16),
                new BatchedNominationForecastCodeLengthRule(16),
            });
    }
}
