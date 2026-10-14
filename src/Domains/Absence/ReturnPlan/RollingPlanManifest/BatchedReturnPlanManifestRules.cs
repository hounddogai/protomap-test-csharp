namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

/// <summary>A rule a batched return plan manifest must pass. Check returns its problems, or none.</summary>
public interface IBatchedReturnPlanManifestRule
{
    List<string> Check(BatchedReturnPlanManifest item);
}

public sealed record BatchedReturnPlanManifestRequiredCodeRule(string Message) : IBatchedReturnPlanManifestRule
{
    public List<string> Check(BatchedReturnPlanManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanManifestRatingRangeRule(double Min, double Max) : IBatchedReturnPlanManifestRule
{
    public List<string> Check(BatchedReturnPlanManifest item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedReturnPlanManifestChecksumLengthRule(int MaxLength) : IBatchedReturnPlanManifestRule
{
    public List<string> Check(BatchedReturnPlanManifest item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedReturnPlanManifestRequiredDescriptionRule(string Message) : IBatchedReturnPlanManifestRule
{
    public List<string> Check(BatchedReturnPlanManifest item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched return plan manifest.</summary>
public sealed record BatchedReturnPlanManifestValidator(List<IBatchedReturnPlanManifestRule> Rules)
{
    public List<string> Validate(BatchedReturnPlanManifest item)
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

    public void RequireValid(BatchedReturnPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedReturnPlanManifest> items)
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

/// <summary>Functions of batched return plan manifest rules.</summary>
public static class BatchedReturnPlanManifestRules
{
    public static BatchedReturnPlanManifestValidator DefaultBatchedReturnPlanManifestValidator()
    {
        return new BatchedReturnPlanManifestValidator(
            new List<IBatchedReturnPlanManifestRule>
            {
                new BatchedReturnPlanManifestRequiredCodeRule("batched return plan manifest needs a value"),
                new BatchedReturnPlanManifestRatingRangeRule(2.7, 3887.3),
                new BatchedReturnPlanManifestChecksumLengthRule(16),
                new BatchedReturnPlanManifestRequiredDescriptionRule("batched return plan manifest needs an entry"),
            });
    }
}
