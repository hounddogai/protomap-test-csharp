namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>A rule a batched reorg snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedReorgSnapshotRule
{
    List<string> Check(BatchedReorgSnapshot item);
}

public sealed record BatchedReorgSnapshotCodeLengthRule(int MaxLength) : IBatchedReorgSnapshotRule
{
    public List<string> Check(BatchedReorgSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReorgSnapshotSalaryBandLengthRule(int MaxLength) : IBatchedReorgSnapshotRule
{
    public List<string> Check(BatchedReorgSnapshot item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReorgSnapshotRequiredSalaryBandRule(string Message) : IBatchedReorgSnapshotRule
{
    public List<string> Check(BatchedReorgSnapshot item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedReorgSnapshotRequiredCodeRule(string Message) : IBatchedReorgSnapshotRule
{
    public List<string> Check(BatchedReorgSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedReorgSnapshotScoreRangeRule(double Min, double Max) : IBatchedReorgSnapshotRule
{
    public List<string> Check(BatchedReorgSnapshot item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched reorg snapshot.</summary>
public sealed record BatchedReorgSnapshotValidator(List<IBatchedReorgSnapshotRule> Rules)
{
    public List<string> Validate(BatchedReorgSnapshot item)
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

    public void RequireValid(BatchedReorgSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedReorgSnapshot> items)
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

/// <summary>Functions of batched reorg snapshot rules.</summary>
public static class BatchedReorgSnapshotRules
{
    public static BatchedReorgSnapshotValidator DefaultBatchedReorgSnapshotValidator()
    {
        return new BatchedReorgSnapshotValidator(
            new List<IBatchedReorgSnapshotRule>
            {
                new BatchedReorgSnapshotCodeLengthRule(64),
                new BatchedReorgSnapshotSalaryBandLengthRule(86400),
                new BatchedReorgSnapshotRequiredSalaryBandRule("batched reorg snapshot needs an entry"),
                new BatchedReorgSnapshotRequiredCodeRule("batched reorg snapshot needs a value"),
                new BatchedReorgSnapshotScoreRangeRule(9.6, 717.2),
            });
    }
}
