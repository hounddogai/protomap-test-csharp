namespace Acme.Workforce.Compliance.Audit.Batch;

/// <summary>A rule a audit batch must pass. Check returns its problems, or none.</summary>
public interface IAuditBatchRule
{
    List<string> Check(AuditBatch item);
}

public sealed record AuditBatchCodePrefixRule(string Prefix) : IAuditBatchRule
{
    public List<string> Check(AuditBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditBatchCreatedAtRangeRule(int Min, int Max) : IAuditBatchRule
{
    public List<string> Check(AuditBatch item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditBatchRequiredTitleRule(string Message) : IAuditBatchRule
{
    public List<string> Check(AuditBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditBatchRetriesRangeRule(int Min, int Max) : IAuditBatchRule
{
    public List<string> Check(AuditBatch item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a audit batch.</summary>
public sealed record AuditBatchValidator(List<IAuditBatchRule> Rules)
{
    public List<string> Validate(AuditBatch item)
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

    public void RequireValid(AuditBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditBatch> items)
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

/// <summary>Functions of audit batch rules.</summary>
public static class AuditBatchRules
{
    public static AuditBatchValidator DefaultAuditBatchValidator()
    {
        return new AuditBatchValidator(
            new List<IAuditBatchRule>
            {
                new AuditBatchCodePrefixRule("ACM"),
                new AuditBatchCreatedAtRangeRule(64, 64),
                new AuditBatchRequiredTitleRule("audit batch needs this field"),
                new AuditBatchRetriesRangeRule(120, 255),
            });
    }
}
