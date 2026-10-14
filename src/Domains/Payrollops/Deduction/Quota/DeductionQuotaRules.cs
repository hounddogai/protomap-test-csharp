namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction quota must pass. Check returns its problems, or none.</summary>
public interface IDeductionQuotaRule
{
    List<string> Check(DeductionQuota item);
}

public sealed record DeductionQuotaCountryLengthRule(int MaxLength) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionQuotaLeaveDaysRangeRule(int Min, int Max) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionQuotaRequiredSourceRule(string Message) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionQuotaRequiredNameRule(string Message) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionQuotaAllowedNameRule(List<string> Allowed) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record DeductionQuotaUpdatedAtRangeRule(int Min, int Max) : IDeductionQuotaRule
{
    public List<string> Check(DeductionQuota item)
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

/// <summary>Runs every rule of a deduction quota.</summary>
public sealed record DeductionQuotaValidator(List<IDeductionQuotaRule> Rules)
{
    public List<string> Validate(DeductionQuota item)
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

    public void RequireValid(DeductionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionQuota> items)
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

/// <summary>Functions of deduction quota rules.</summary>
public static class DeductionQuotaRules
{
    public static DeductionQuotaValidator DefaultDeductionQuotaValidator()
    {
        return new DeductionQuotaValidator(
            new List<IDeductionQuotaRule>
            {
                new DeductionQuotaCountryLengthRule(16),
                new DeductionQuotaLeaveDaysRangeRule(16, 1000),
                new DeductionQuotaRequiredSourceRule("deduction quota needs an entry"),
                new DeductionQuotaRequiredNameRule("deduction quota needs a value"),
                new DeductionQuotaAllowedNameRule(new List<string> { "legacy", "seasonal" }),
                new DeductionQuotaUpdatedAtRangeRule(255, 86400),
            });
    }
}
