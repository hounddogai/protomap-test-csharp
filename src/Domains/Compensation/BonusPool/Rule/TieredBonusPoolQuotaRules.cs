namespace Acme.Workforce.Compensation.BonusPool.Rule;

/// <summary>A rule a tiered bonus pool quota must pass. Check returns its problems, or none.</summary>
public interface ITieredBonusPoolQuotaRule
{
    List<string> Check(TieredBonusPoolQuota item);
}

public sealed record TieredBonusPoolQuotaCodePrefixRule(string Prefix) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolQuotaMarginRangeRule(double Min, double Max) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredBonusPoolQuotaSummaryLengthRule(int MaxLength) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolQuotaRequiredContactEmailRule(string Message) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolQuotaRequiredNameRule(string Message) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredBonusPoolQuotaContactEmailLengthRule(int MaxLength) : ITieredBonusPoolQuotaRule
{
    public List<string> Check(TieredBonusPoolQuota item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered bonus pool quota.</summary>
public sealed record TieredBonusPoolQuotaValidator(List<ITieredBonusPoolQuotaRule> Rules)
{
    public List<string> Validate(TieredBonusPoolQuota item)
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

    public void RequireValid(TieredBonusPoolQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredBonusPoolQuota> items)
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

/// <summary>Functions of tiered bonus pool quota rules.</summary>
public static class TieredBonusPoolQuotaRules
{
    public static TieredBonusPoolQuotaValidator DefaultTieredBonusPoolQuotaValidator()
    {
        return new TieredBonusPoolQuotaValidator(
            new List<ITieredBonusPoolQuotaRule>
            {
                new TieredBonusPoolQuotaCodePrefixRule("REF"),
                new TieredBonusPoolQuotaMarginRangeRule(9.6, 362.2),
                new TieredBonusPoolQuotaSummaryLengthRule(1000),
                new TieredBonusPoolQuotaRequiredContactEmailRule("tiered bonus pool quota needs this field"),
                new TieredBonusPoolQuotaRequiredNameRule("tiered bonus pool quota needs this field"),
                new TieredBonusPoolQuotaContactEmailLengthRule(120),
            });
    }
}
