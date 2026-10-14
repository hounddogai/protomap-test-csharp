namespace Acme.Workforce.Absence.Balance.Summary;

/// <summary>A rule a balance quota must pass. Check returns its problems, or none.</summary>
public interface IBalanceQuotaRule
{
    List<string> Check(BalanceQuota item);
}

public sealed record BalanceQuotaCodeLengthRule(int MaxLength) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceQuotaTimezoneLengthRule(int MaxLength) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceQuotaPhoneLengthRule(int MaxLength) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceQuotaRequiredPhoneRule(string Message) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceQuotaRequiredLabelRule(string Message) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceQuotaWindowDaysRangeRule(int Min, int Max) : IBalanceQuotaRule
{
    public List<string> Check(BalanceQuota item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance quota.</summary>
public sealed record BalanceQuotaValidator(List<IBalanceQuotaRule> Rules)
{
    public List<string> Validate(BalanceQuota item)
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

    public void RequireValid(BalanceQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceQuota> items)
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

/// <summary>Functions of balance quota rules.</summary>
public static class BalanceQuotaRules
{
    public static BalanceQuotaValidator DefaultBalanceQuotaValidator()
    {
        return new BalanceQuotaValidator(
            new List<IBalanceQuotaRule>
            {
                new BalanceQuotaCodeLengthRule(16),
                new BalanceQuotaTimezoneLengthRule(86400),
                new BalanceQuotaPhoneLengthRule(80),
                new BalanceQuotaRequiredPhoneRule("balance quota needs a value"),
                new BalanceQuotaRequiredLabelRule("balance quota needs an entry"),
                new BalanceQuotaWindowDaysRangeRule(16, 1000),
            });
    }
}
