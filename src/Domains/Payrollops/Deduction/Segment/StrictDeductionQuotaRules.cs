namespace Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>A rule a strict deduction quota must pass. Check returns its problems, or none.</summary>
public interface IStrictDeductionQuotaRule
{
    List<string> Check(StrictDeductionQuota item);
}

public sealed record StrictDeductionQuotaRetriesRangeRule(int Min, int Max) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionQuotaCeilingRangeRule(double Min, double Max) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionQuotaLabelLengthRule(int MaxLength) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionQuotaCodeLengthRule(int MaxLength) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionQuotaRequiredLabelRule(string Message) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionQuotaCurrencyLengthRule(int MaxLength) : IStrictDeductionQuotaRule
{
    public List<string> Check(StrictDeductionQuota item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict deduction quota.</summary>
public sealed record StrictDeductionQuotaValidator(List<IStrictDeductionQuotaRule> Rules)
{
    public List<string> Validate(StrictDeductionQuota item)
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

    public void RequireValid(StrictDeductionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeductionQuota> items)
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

/// <summary>Functions of strict deduction quota rules.</summary>
public static class StrictDeductionQuotaRules
{
    public static StrictDeductionQuotaValidator DefaultStrictDeductionQuotaValidator()
    {
        return new StrictDeductionQuotaValidator(
            new List<IStrictDeductionQuotaRule>
            {
                new StrictDeductionQuotaRetriesRangeRule(120, 255),
                new StrictDeductionQuotaCeilingRangeRule(1.3, 2550.0),
                new StrictDeductionQuotaLabelLengthRule(255),
                new StrictDeductionQuotaCodeLengthRule(64),
                new StrictDeductionQuotaRequiredLabelRule("strict deduction quota needs this field"),
                new StrictDeductionQuotaCurrencyLengthRule(3600),
            });
    }
}
