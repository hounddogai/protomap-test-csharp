namespace Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>A rule a batched tax form segment must pass. Check returns its problems, or none.</summary>
public interface IBatchedTaxFormSegmentRule
{
    List<string> Check(BatchedTaxFormSegment item);
}

public sealed record BatchedTaxFormSegmentRequiredSkillRule(string Message) : IBatchedTaxFormSegmentRule
{
    public List<string> Check(BatchedTaxFormSegment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormSegmentRequiredLabelRule(string Message) : IBatchedTaxFormSegmentRule
{
    public List<string> Check(BatchedTaxFormSegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormSegmentPinnedGuardRule(int Limit) : IBatchedTaxFormSegmentRule
{
    public List<string> Check(BatchedTaxFormSegment item)
    {
        if (item.Pinned && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched tax form segment.</summary>
public sealed record BatchedTaxFormSegmentValidator(List<IBatchedTaxFormSegmentRule> Rules)
{
    public List<string> Validate(BatchedTaxFormSegment item)
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

    public void RequireValid(BatchedTaxFormSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTaxFormSegment> items)
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

/// <summary>Functions of batched tax form segment rules.</summary>
public static class BatchedTaxFormSegmentRules
{
    public static BatchedTaxFormSegmentValidator DefaultBatchedTaxFormSegmentValidator()
    {
        return new BatchedTaxFormSegmentValidator(
            new List<IBatchedTaxFormSegmentRule>
            {
                new BatchedTaxFormSegmentRequiredSkillRule("batched tax form segment needs a value"),
                new BatchedTaxFormSegmentRequiredLabelRule("batched tax form segment needs a value"),
                new BatchedTaxFormSegmentPinnedGuardRule(86400),
            });
    }
}
