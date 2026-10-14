namespace Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>A rule a pending career path batch must pass. Check returns its problems, or none.</summary>
public interface IPendingCareerPathBatchRule
{
    List<string> Check(PendingCareerPathBatch item);
}

public sealed record PendingCareerPathBatchBatchSizeRangeRule(int Min, int Max) : IPendingCareerPathBatchRule
{
    public List<string> Check(PendingCareerPathBatch item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathBatchCreatedAtRangeRule(int Min, int Max) : IPendingCareerPathBatchRule
{
    public List<string> Check(PendingCareerPathBatch item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathBatchCodePrefixRule(string Prefix) : IPendingCareerPathBatchRule
{
    public List<string> Check(PendingCareerPathBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathBatchEmployeeIdLengthRule(int MaxLength) : IPendingCareerPathBatchRule
{
    public List<string> Check(PendingCareerPathBatch item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingCareerPathBatchRequiredCodeRule(string Message) : IPendingCareerPathBatchRule
{
    public List<string> Check(PendingCareerPathBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending career path batch.</summary>
public sealed record PendingCareerPathBatchValidator(List<IPendingCareerPathBatchRule> Rules)
{
    public List<string> Validate(PendingCareerPathBatch item)
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

    public void RequireValid(PendingCareerPathBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingCareerPathBatch> items)
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

/// <summary>Functions of pending career path batch rules.</summary>
public static class PendingCareerPathBatchRules
{
    public static PendingCareerPathBatchValidator DefaultPendingCareerPathBatchValidator()
    {
        return new PendingCareerPathBatchValidator(
            new List<IPendingCareerPathBatchRule>
            {
                new PendingCareerPathBatchBatchSizeRangeRule(120, 1000),
                new PendingCareerPathBatchCreatedAtRangeRule(64, 255),
                new PendingCareerPathBatchCodePrefixRule("X"),
                new PendingCareerPathBatchEmployeeIdLengthRule(120),
                new PendingCareerPathBatchRequiredCodeRule("pending career path batch needs this field"),
            });
    }
}
