namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh quota must pass. Check returns its problems, or none.</summary>
public interface IRefreshQuotaRule
{
    List<string> Check(RefreshQuota item);
}

public sealed record RefreshQuotaCodePrefixRule(string Prefix) : IRefreshQuotaRule
{
    public List<string> Check(RefreshQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshQuotaCodeLengthRule(int MaxLength) : IRefreshQuotaRule
{
    public List<string> Check(RefreshQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshQuotaCeilingRangeRule(double Min, double Max) : IRefreshQuotaRule
{
    public List<string> Check(RefreshQuota item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshQuotaRequiredCountryRule(string Message) : IRefreshQuotaRule
{
    public List<string> Check(RefreshQuota item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshQuotaAllowedCountryRule(List<string> Allowed) : IRefreshQuotaRule
{
    public List<string> Check(RefreshQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh quota.</summary>
public sealed record RefreshQuotaValidator(List<IRefreshQuotaRule> Rules)
{
    public List<string> Validate(RefreshQuota item)
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

    public void RequireValid(RefreshQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshQuota> items)
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

/// <summary>Functions of refresh quota rules.</summary>
public static class RefreshQuotaRules
{
    public static RefreshQuotaValidator DefaultRefreshQuotaValidator()
    {
        return new RefreshQuotaValidator(
            new List<IRefreshQuotaRule>
            {
                new RefreshQuotaCodePrefixRule("OPS"),
                new RefreshQuotaCodeLengthRule(1000),
                new RefreshQuotaCeilingRangeRule(3.0, 1332.9),
                new RefreshQuotaRequiredCountryRule("refresh quota needs this field"),
                new RefreshQuotaAllowedCountryRule(new List<string> { "manual", "lenient", "primary" }),
            });
    }
}
