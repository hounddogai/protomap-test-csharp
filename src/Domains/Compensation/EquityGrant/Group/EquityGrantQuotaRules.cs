namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant quota must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantQuotaRule
{
    List<string> Check(EquityGrantQuota item);
}

public sealed record EquityGrantQuotaUpdatedAtRangeRule(int Min, int Max) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantQuotaUnitLengthRule(int MaxLength) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantQuotaQuantityRangeRule(int Min, int Max) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantQuotaRequiredNameRule(string Message) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantQuotaRequiredCodeRule(string Message) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantQuotaAllowedTimezoneRule(List<string> Allowed) : IEquityGrantQuotaRule
{
    public List<string> Check(EquityGrantQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant quota.</summary>
public sealed record EquityGrantQuotaValidator(List<IEquityGrantQuotaRule> Rules)
{
    public List<string> Validate(EquityGrantQuota item)
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

    public void RequireValid(EquityGrantQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantQuota> items)
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

/// <summary>Functions of equity grant quota rules.</summary>
public static class EquityGrantQuotaRules
{
    public static EquityGrantQuotaValidator DefaultEquityGrantQuotaValidator()
    {
        return new EquityGrantQuotaValidator(
            new List<IEquityGrantQuotaRule>
            {
                new EquityGrantQuotaUpdatedAtRangeRule(16, 32),
                new EquityGrantQuotaUnitLengthRule(120),
                new EquityGrantQuotaQuantityRangeRule(16, 64),
                new EquityGrantQuotaRequiredNameRule("equity grant quota needs a value"),
                new EquityGrantQuotaRequiredCodeRule("equity grant quota needs a value"),
                new EquityGrantQuotaAllowedTimezoneRule(new List<string> { "scheduled", "scheduled" }),
            });
    }
}
