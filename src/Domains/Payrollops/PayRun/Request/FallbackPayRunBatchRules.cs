namespace Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>A rule a fallback pay run batch must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayRunBatchRule
{
    List<string> Check(FallbackPayRunBatch item);
}

public sealed record FallbackPayRunBatchRequiredCodeRule(string Message) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunBatchCodeLengthRule(int MaxLength) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunBatchNameLengthRule(int MaxLength) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunBatchSourceLengthRule(int MaxLength) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunBatchRequiredNameRule(string Message) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayRunBatchRequiredSourceRule(string Message) : IFallbackPayRunBatchRule
{
    public List<string> Check(FallbackPayRunBatch item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay run batch.</summary>
public sealed record FallbackPayRunBatchValidator(List<IFallbackPayRunBatchRule> Rules)
{
    public List<string> Validate(FallbackPayRunBatch item)
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

    public void RequireValid(FallbackPayRunBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayRunBatch> items)
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

/// <summary>Functions of fallback pay run batch rules.</summary>
public static class FallbackPayRunBatchRules
{
    public static FallbackPayRunBatchValidator DefaultFallbackPayRunBatchValidator()
    {
        return new FallbackPayRunBatchValidator(
            new List<IFallbackPayRunBatchRule>
            {
                new FallbackPayRunBatchRequiredCodeRule("fallback pay run batch needs an entry"),
                new FallbackPayRunBatchCodeLengthRule(32),
                new FallbackPayRunBatchNameLengthRule(1000),
                new FallbackPayRunBatchSourceLengthRule(86400),
                new FallbackPayRunBatchRequiredNameRule("fallback pay run batch needs this field"),
                new FallbackPayRunBatchRequiredSourceRule("fallback pay run batch needs a value"),
            });
    }
}
