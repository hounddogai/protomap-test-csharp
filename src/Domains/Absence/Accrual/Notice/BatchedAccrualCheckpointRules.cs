namespace Acme.Workforce.Absence.Accrual.Notice;

/// <summary>A rule a batched accrual checkpoint must pass. Check returns its problems, or none.</summary>
public interface IBatchedAccrualCheckpointRule
{
    List<string> Check(BatchedAccrualCheckpoint item);
}

public sealed record BatchedAccrualCheckpointMarginRangeRule(double Min, double Max) : IBatchedAccrualCheckpointRule
{
    public List<string> Check(BatchedAccrualCheckpoint item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualCheckpointCodePrefixRule(string Prefix) : IBatchedAccrualCheckpointRule
{
    public List<string> Check(BatchedAccrualCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualCheckpointLabelLengthRule(int MaxLength) : IBatchedAccrualCheckpointRule
{
    public List<string> Check(BatchedAccrualCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAccrualCheckpointSourceLengthRule(int MaxLength) : IBatchedAccrualCheckpointRule
{
    public List<string> Check(BatchedAccrualCheckpoint item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched accrual checkpoint.</summary>
public sealed record BatchedAccrualCheckpointValidator(List<IBatchedAccrualCheckpointRule> Rules)
{
    public List<string> Validate(BatchedAccrualCheckpoint item)
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

    public void RequireValid(BatchedAccrualCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAccrualCheckpoint> items)
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

/// <summary>Functions of batched accrual checkpoint rules.</summary>
public static class BatchedAccrualCheckpointRules
{
    public static BatchedAccrualCheckpointValidator DefaultBatchedAccrualCheckpointValidator()
    {
        return new BatchedAccrualCheckpointValidator(
            new List<IBatchedAccrualCheckpointRule>
            {
                new BatchedAccrualCheckpointMarginRangeRule(3.2, 4696.2),
                new BatchedAccrualCheckpointCodePrefixRule("OPS"),
                new BatchedAccrualCheckpointLabelLengthRule(1000),
                new BatchedAccrualCheckpointSourceLengthRule(32),
            });
    }
}
