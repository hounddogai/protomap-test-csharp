namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>A rule a lenient equity grant batch must pass. Check returns its problems, or none.</summary>
public interface ILenientEquityGrantBatchRule
{
    List<string> Check(LenientEquityGrantBatch item);
}

public sealed record LenientEquityGrantBatchRequiredNameRule(string Message) : ILenientEquityGrantBatchRule
{
    public List<string> Check(LenientEquityGrantBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantBatchRequiredCodeRule(string Message) : ILenientEquityGrantBatchRule
{
    public List<string> Check(LenientEquityGrantBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantBatchCodeLengthRule(int MaxLength) : ILenientEquityGrantBatchRule
{
    public List<string> Check(LenientEquityGrantBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantBatchRequiredUnitRule(string Message) : ILenientEquityGrantBatchRule
{
    public List<string> Check(LenientEquityGrantBatch item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientEquityGrantBatchJobCodeLengthRule(int MaxLength) : ILenientEquityGrantBatchRule
{
    public List<string> Check(LenientEquityGrantBatch item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient equity grant batch.</summary>
public sealed record LenientEquityGrantBatchValidator(List<ILenientEquityGrantBatchRule> Rules)
{
    public List<string> Validate(LenientEquityGrantBatch item)
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

    public void RequireValid(LenientEquityGrantBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientEquityGrantBatch> items)
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

/// <summary>Functions of lenient equity grant batch rules.</summary>
public static class LenientEquityGrantBatchRules
{
    public static LenientEquityGrantBatchValidator DefaultLenientEquityGrantBatchValidator()
    {
        return new LenientEquityGrantBatchValidator(
            new List<ILenientEquityGrantBatchRule>
            {
                new LenientEquityGrantBatchRequiredNameRule("lenient equity grant batch needs a value"),
                new LenientEquityGrantBatchRequiredCodeRule("lenient equity grant batch needs this field"),
                new LenientEquityGrantBatchCodeLengthRule(255),
                new LenientEquityGrantBatchRequiredUnitRule("lenient equity grant batch needs this field"),
                new LenientEquityGrantBatchJobCodeLengthRule(32),
            });
    }
}
