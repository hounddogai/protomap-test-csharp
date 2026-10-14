namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>A rule a capped visitor batch must pass. Check returns its problems, or none.</summary>
public interface ICappedVisitorBatchRule
{
    List<string> Check(CappedVisitorBatch item);
}

public sealed record CappedVisitorBatchRequiredPhoneRule(string Message) : ICappedVisitorBatchRule
{
    public List<string> Check(CappedVisitorBatch item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorBatchTitleLengthRule(int MaxLength) : ICappedVisitorBatchRule
{
    public List<string> Check(CappedVisitorBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedVisitorBatchCodeLengthRule(int MaxLength) : ICappedVisitorBatchRule
{
    public List<string> Check(CappedVisitorBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped visitor batch.</summary>
public sealed record CappedVisitorBatchValidator(List<ICappedVisitorBatchRule> Rules)
{
    public List<string> Validate(CappedVisitorBatch item)
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

    public void RequireValid(CappedVisitorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedVisitorBatch> items)
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

/// <summary>Functions of capped visitor batch rules.</summary>
public static class CappedVisitorBatchRules
{
    public static CappedVisitorBatchValidator DefaultCappedVisitorBatchValidator()
    {
        return new CappedVisitorBatchValidator(
            new List<ICappedVisitorBatchRule>
            {
                new CappedVisitorBatchRequiredPhoneRule("capped visitor batch needs an entry"),
                new CappedVisitorBatchTitleLengthRule(32),
                new CappedVisitorBatchCodeLengthRule(16),
            });
    }
}
