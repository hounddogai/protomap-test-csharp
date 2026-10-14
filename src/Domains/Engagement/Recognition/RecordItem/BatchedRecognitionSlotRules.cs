namespace Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>A rule a batched recognition slot must pass. Check returns its problems, or none.</summary>
public interface IBatchedRecognitionSlotRule
{
    List<string> Check(BatchedRecognitionSlot item);
}

public sealed record BatchedRecognitionSlotRequiredOwnerIdRule(string Message) : IBatchedRecognitionSlotRule
{
    public List<string> Check(BatchedRecognitionSlot item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionSlotCodePrefixRule(string Prefix) : IBatchedRecognitionSlotRule
{
    public List<string> Check(BatchedRecognitionSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRecognitionSlotOwnerIdLengthRule(int MaxLength) : IBatchedRecognitionSlotRule
{
    public List<string> Check(BatchedRecognitionSlot item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched recognition slot.</summary>
public sealed record BatchedRecognitionSlotValidator(List<IBatchedRecognitionSlotRule> Rules)
{
    public List<string> Validate(BatchedRecognitionSlot item)
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

    public void RequireValid(BatchedRecognitionSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRecognitionSlot> items)
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

/// <summary>Functions of batched recognition slot rules.</summary>
public static class BatchedRecognitionSlotRules
{
    public static BatchedRecognitionSlotValidator DefaultBatchedRecognitionSlotValidator()
    {
        return new BatchedRecognitionSlotValidator(
            new List<IBatchedRecognitionSlotRule>
            {
                new BatchedRecognitionSlotRequiredOwnerIdRule("batched recognition slot needs this field"),
                new BatchedRecognitionSlotCodePrefixRule("OPS"),
                new BatchedRecognitionSlotOwnerIdLengthRule(255),
            });
    }
}
