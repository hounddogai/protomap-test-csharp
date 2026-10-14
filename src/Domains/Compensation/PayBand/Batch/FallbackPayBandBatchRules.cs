namespace Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>A rule a fallback pay band batch must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayBandBatchRule
{
    List<string> Check(FallbackPayBandBatch item);
}

public sealed record FallbackPayBandBatchCodeLengthRule(int MaxLength) : IFallbackPayBandBatchRule
{
    public List<string> Check(FallbackPayBandBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandBatchChecksumLengthRule(int MaxLength) : IFallbackPayBandBatchRule
{
    public List<string> Check(FallbackPayBandBatch item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandBatchBudgetRangeRule(double Min, double Max) : IFallbackPayBandBatchRule
{
    public List<string> Check(FallbackPayBandBatch item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandBatchRequiredCodeRule(string Message) : IFallbackPayBandBatchRule
{
    public List<string> Check(FallbackPayBandBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay band batch.</summary>
public sealed record FallbackPayBandBatchValidator(List<IFallbackPayBandBatchRule> Rules)
{
    public List<string> Validate(FallbackPayBandBatch item)
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

    public void RequireValid(FallbackPayBandBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayBandBatch> items)
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

/// <summary>Functions of fallback pay band batch rules.</summary>
public static class FallbackPayBandBatchRules
{
    public static FallbackPayBandBatchValidator DefaultFallbackPayBandBatchValidator()
    {
        return new FallbackPayBandBatchValidator(
            new List<IFallbackPayBandBatchRule>
            {
                new FallbackPayBandBatchCodeLengthRule(86400),
                new FallbackPayBandBatchChecksumLengthRule(64),
                new FallbackPayBandBatchBudgetRangeRule(3.7, 1894.8),
                new FallbackPayBandBatchRequiredCodeRule("fallback pay band batch needs an entry"),
            });
    }
}
