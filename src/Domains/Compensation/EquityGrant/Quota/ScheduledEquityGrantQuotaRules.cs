namespace Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>A rule a scheduled equity grant quota must pass. Check returns its problems, or none.</summary>
public interface IScheduledEquityGrantQuotaRule
{
    List<string> Check(ScheduledEquityGrantQuota item);
}

public sealed record ScheduledEquityGrantQuotaRequiredCategoryRule(string Message) : IScheduledEquityGrantQuotaRule
{
    public List<string> Check(ScheduledEquityGrantQuota item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantQuotaCodeLengthRule(int MaxLength) : IScheduledEquityGrantQuotaRule
{
    public List<string> Check(ScheduledEquityGrantQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantQuotaRequiredCurrencyRule(string Message) : IScheduledEquityGrantQuotaRule
{
    public List<string> Check(ScheduledEquityGrantQuota item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantQuotaTitleLengthRule(int MaxLength) : IScheduledEquityGrantQuotaRule
{
    public List<string> Check(ScheduledEquityGrantQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEquityGrantQuotaQuantityRangeRule(int Min, int Max) : IScheduledEquityGrantQuotaRule
{
    public List<string> Check(ScheduledEquityGrantQuota item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled equity grant quota.</summary>
public sealed record ScheduledEquityGrantQuotaValidator(List<IScheduledEquityGrantQuotaRule> Rules)
{
    public List<string> Validate(ScheduledEquityGrantQuota item)
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

    public void RequireValid(ScheduledEquityGrantQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEquityGrantQuota> items)
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

/// <summary>Functions of scheduled equity grant quota rules.</summary>
public static class ScheduledEquityGrantQuotaRules
{
    public static ScheduledEquityGrantQuotaValidator DefaultScheduledEquityGrantQuotaValidator()
    {
        return new ScheduledEquityGrantQuotaValidator(
            new List<IScheduledEquityGrantQuotaRule>
            {
                new ScheduledEquityGrantQuotaRequiredCategoryRule("scheduled equity grant quota needs an entry"),
                new ScheduledEquityGrantQuotaCodeLengthRule(255),
                new ScheduledEquityGrantQuotaRequiredCurrencyRule("scheduled equity grant quota needs an entry"),
                new ScheduledEquityGrantQuotaTitleLengthRule(64),
                new ScheduledEquityGrantQuotaQuantityRangeRule(3600, 86400),
            });
    }
}
