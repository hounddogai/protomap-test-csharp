namespace Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>A rule a tiered garnishment quota must pass. Check returns its problems, or none.</summary>
public interface ITieredGarnishmentQuotaRule
{
    List<string> Check(TieredGarnishmentQuota item);
}

public sealed record TieredGarnishmentQuotaMaximumRangeRule(double Min, double Max) : ITieredGarnishmentQuotaRule
{
    public List<string> Check(TieredGarnishmentQuota item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredGarnishmentQuotaRequiredCodeRule(string Message) : ITieredGarnishmentQuotaRule
{
    public List<string> Check(TieredGarnishmentQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredGarnishmentQuotaCodePrefixRule(string Prefix) : ITieredGarnishmentQuotaRule
{
    public List<string> Check(TieredGarnishmentQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredGarnishmentQuotaRequiredTitleRule(string Message) : ITieredGarnishmentQuotaRule
{
    public List<string> Check(TieredGarnishmentQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered garnishment quota.</summary>
public sealed record TieredGarnishmentQuotaValidator(List<ITieredGarnishmentQuotaRule> Rules)
{
    public List<string> Validate(TieredGarnishmentQuota item)
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

    public void RequireValid(TieredGarnishmentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredGarnishmentQuota> items)
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

/// <summary>Functions of tiered garnishment quota rules.</summary>
public static class TieredGarnishmentQuotaRules
{
    public static TieredGarnishmentQuotaValidator DefaultTieredGarnishmentQuotaValidator()
    {
        return new TieredGarnishmentQuotaValidator(
            new List<ITieredGarnishmentQuotaRule>
            {
                new TieredGarnishmentQuotaMaximumRangeRule(0.2, 2250.0),
                new TieredGarnishmentQuotaRequiredCodeRule("tiered garnishment quota needs a value"),
                new TieredGarnishmentQuotaCodePrefixRule("Q"),
                new TieredGarnishmentQuotaRequiredTitleRule("tiered garnishment quota needs an entry"),
            });
    }
}
