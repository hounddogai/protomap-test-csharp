namespace Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>A rule a capped balance quota must pass. Check returns its problems, or none.</summary>
public interface ICappedBalanceQuotaRule
{
    List<string> Check(CappedBalanceQuota item);
}

public sealed record CappedBalanceQuotaCostRangeRule(double Min, double Max) : ICappedBalanceQuotaRule
{
    public List<string> Check(CappedBalanceQuota item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceQuotaLabelLengthRule(int MaxLength) : ICappedBalanceQuotaRule
{
    public List<string> Check(CappedBalanceQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceQuotaRequiredLabelRule(string Message) : ICappedBalanceQuotaRule
{
    public List<string> Check(CappedBalanceQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceQuotaSalaryBandLengthRule(int MaxLength) : ICappedBalanceQuotaRule
{
    public List<string> Check(CappedBalanceQuota item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedBalanceQuotaCodePrefixRule(string Prefix) : ICappedBalanceQuotaRule
{
    public List<string> Check(CappedBalanceQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped balance quota.</summary>
public sealed record CappedBalanceQuotaValidator(List<ICappedBalanceQuotaRule> Rules)
{
    public List<string> Validate(CappedBalanceQuota item)
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

    public void RequireValid(CappedBalanceQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedBalanceQuota> items)
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

/// <summary>Functions of capped balance quota rules.</summary>
public static class CappedBalanceQuotaRules
{
    public static CappedBalanceQuotaValidator DefaultCappedBalanceQuotaValidator()
    {
        return new CappedBalanceQuotaValidator(
            new List<ICappedBalanceQuotaRule>
            {
                new CappedBalanceQuotaCostRangeRule(1.8, 2196.4),
                new CappedBalanceQuotaLabelLengthRule(64),
                new CappedBalanceQuotaRequiredLabelRule("capped balance quota needs a value"),
                new CappedBalanceQuotaSalaryBandLengthRule(1000),
                new CappedBalanceQuotaCodePrefixRule("X"),
            });
    }
}
