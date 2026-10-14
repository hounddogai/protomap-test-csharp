namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>A rule a batched warranty draft must pass. Check returns its problems, or none.</summary>
public interface IBatchedWarrantyDraftRule
{
    List<string> Check(BatchedWarrantyDraft item);
}

public sealed record BatchedWarrantyDraftRequiredLocationRule(string Message) : IBatchedWarrantyDraftRule
{
    public List<string> Check(BatchedWarrantyDraft item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyDraftCeilingRangeRule(double Min, double Max) : IBatchedWarrantyDraftRule
{
    public List<string> Check(BatchedWarrantyDraft item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyDraftVersionRangeRule(int Min, int Max) : IBatchedWarrantyDraftRule
{
    public List<string> Check(BatchedWarrantyDraft item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyDraftMinimumRangeRule(double Min, double Max) : IBatchedWarrantyDraftRule
{
    public List<string> Check(BatchedWarrantyDraft item)
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

public sealed record BatchedWarrantyDraftDurationMinutesRangeRule(int Min, int Max) : IBatchedWarrantyDraftRule
{
    public List<string> Check(BatchedWarrantyDraft item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched warranty draft.</summary>
public sealed record BatchedWarrantyDraftValidator(List<IBatchedWarrantyDraftRule> Rules)
{
    public List<string> Validate(BatchedWarrantyDraft item)
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

    public void RequireValid(BatchedWarrantyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedWarrantyDraft> items)
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

/// <summary>Functions of batched warranty draft rules.</summary>
public static class BatchedWarrantyDraftRules
{
    public static BatchedWarrantyDraftValidator DefaultBatchedWarrantyDraftValidator()
    {
        return new BatchedWarrantyDraftValidator(
            new List<IBatchedWarrantyDraftRule>
            {
                new BatchedWarrantyDraftRequiredLocationRule("batched warranty draft needs an entry"),
                new BatchedWarrantyDraftCeilingRangeRule(6.5, 3615.1),
                new BatchedWarrantyDraftVersionRangeRule(16, 255),
                new BatchedWarrantyDraftMinimumRangeRule(5.8, 4572.6),
                new BatchedWarrantyDraftDurationMinutesRangeRule(80, 1000),
            });
    }
}
