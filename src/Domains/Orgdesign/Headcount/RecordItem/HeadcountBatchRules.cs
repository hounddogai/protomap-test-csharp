namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>A rule a headcount batch must pass. Check returns its problems, or none.</summary>
public interface IHeadcountBatchRule
{
    List<string> Check(HeadcountBatch item);
}

public sealed record HeadcountBatchDepartmentLengthRule(int MaxLength) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBatchArchivedGuardRule(int Limit) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        if (item.Archived && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBatchCodePrefixRule(string Prefix) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBatchRequiredCodeRule(string Message) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBatchNameLengthRule(int MaxLength) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBatchCodeLengthRule(int MaxLength) : IHeadcountBatchRule
{
    public List<string> Check(HeadcountBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount batch.</summary>
public sealed record HeadcountBatchValidator(List<IHeadcountBatchRule> Rules)
{
    public List<string> Validate(HeadcountBatch item)
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

    public void RequireValid(HeadcountBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountBatch> items)
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

/// <summary>Functions of headcount batch rules.</summary>
public static class HeadcountBatchRules
{
    public static HeadcountBatchValidator DefaultHeadcountBatchValidator()
    {
        return new HeadcountBatchValidator(
            new List<IHeadcountBatchRule>
            {
                new HeadcountBatchDepartmentLengthRule(86400),
                new HeadcountBatchArchivedGuardRule(16),
                new HeadcountBatchCodePrefixRule("Q"),
                new HeadcountBatchRequiredCodeRule("headcount batch needs this field"),
                new HeadcountBatchNameLengthRule(3600),
                new HeadcountBatchCodeLengthRule(32),
            });
    }
}
