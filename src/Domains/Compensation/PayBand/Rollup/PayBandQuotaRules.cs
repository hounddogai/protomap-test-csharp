namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band quota must pass. Check returns its problems, or none.</summary>
public interface IPayBandQuotaRule
{
    List<string> Check(PayBandQuota item);
}

public sealed record PayBandQuotaRequiredCodeRule(string Message) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandQuotaRequiredRegionRule(string Message) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandQuotaAllowedCodeRule(List<string> Allowed) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PayBandQuotaCodeLengthRule(int MaxLength) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandQuotaAmountRangeRule(double Min, double Max) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandQuotaExpiresAtRangeRule(int Min, int Max) : IPayBandQuotaRule
{
    public List<string> Check(PayBandQuota item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band quota.</summary>
public sealed record PayBandQuotaValidator(List<IPayBandQuotaRule> Rules)
{
    public List<string> Validate(PayBandQuota item)
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

    public void RequireValid(PayBandQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandQuota> items)
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

/// <summary>Functions of pay band quota rules.</summary>
public static class PayBandQuotaRules
{
    public static PayBandQuotaValidator DefaultPayBandQuotaValidator()
    {
        return new PayBandQuotaValidator(
            new List<IPayBandQuotaRule>
            {
                new PayBandQuotaRequiredCodeRule("pay band quota needs this field"),
                new PayBandQuotaRequiredRegionRule("pay band quota needs this field"),
                new PayBandQuotaAllowedCodeRule(new List<string> { "weighted", "rolling", "weighted" }),
                new PayBandQuotaCodeLengthRule(32),
                new PayBandQuotaAmountRangeRule(10.0, 3738.4),
                new PayBandQuotaExpiresAtRangeRule(32, 86400),
            });
    }
}
