namespace Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>A rule a batched warranty record must pass. Check returns its problems, or none.</summary>
public interface IBatchedWarrantyRecordRule
{
    List<string> Check(BatchedWarrantyRecord item);
}

public sealed record BatchedWarrantyRecordReviewerLengthRule(int MaxLength) : IBatchedWarrantyRecordRule
{
    public List<string> Check(BatchedWarrantyRecord item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyRecordTitleLengthRule(int MaxLength) : IBatchedWarrantyRecordRule
{
    public List<string> Check(BatchedWarrantyRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyRecordRequiredCodeRule(string Message) : IBatchedWarrantyRecordRule
{
    public List<string> Check(BatchedWarrantyRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyRecordMinimumRangeRule(double Min, double Max) : IBatchedWarrantyRecordRule
{
    public List<string> Check(BatchedWarrantyRecord item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched warranty record.</summary>
public sealed record BatchedWarrantyRecordValidator(List<IBatchedWarrantyRecordRule> Rules)
{
    public List<string> Validate(BatchedWarrantyRecord item)
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

    public void RequireValid(BatchedWarrantyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedWarrantyRecord> items)
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

/// <summary>Functions of batched warranty record rules.</summary>
public static class BatchedWarrantyRecordRules
{
    public static BatchedWarrantyRecordValidator DefaultBatchedWarrantyRecordValidator()
    {
        return new BatchedWarrantyRecordValidator(
            new List<IBatchedWarrantyRecordRule>
            {
                new BatchedWarrantyRecordReviewerLengthRule(255),
                new BatchedWarrantyRecordTitleLengthRule(86400),
                new BatchedWarrantyRecordRequiredCodeRule("batched warranty record needs an entry"),
                new BatchedWarrantyRecordMinimumRangeRule(0.3, 834.9),
            });
    }
}
