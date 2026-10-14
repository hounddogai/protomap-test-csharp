namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pool metric must pass. Check returns its problems, or none.</summary>
public interface ITieredPoolMetricRule
{
    List<string> Check(TieredPoolMetric item);
}

public sealed record TieredPoolMetricAllowedExternalRefRule(List<string> Allowed) : ITieredPoolMetricRule
{
    public List<string> Check(TieredPoolMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolMetricRequiredLabelRule(string Message) : ITieredPoolMetricRule
{
    public List<string> Check(TieredPoolMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolMetricPinnedGuardRule(int Limit) : ITieredPoolMetricRule
{
    public List<string> Check(TieredPoolMetric item)
    {
        if (item.Pinned && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolMetricCodePrefixRule(string Prefix) : ITieredPoolMetricRule
{
    public List<string> Check(TieredPoolMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pool metric.</summary>
public sealed record TieredPoolMetricValidator(List<ITieredPoolMetricRule> Rules)
{
    public List<string> Validate(TieredPoolMetric item)
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

    public void RequireValid(TieredPoolMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPoolMetric> items)
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

/// <summary>Functions of tiered pool metric rules.</summary>
public static class TieredPoolMetricRules
{
    public static TieredPoolMetricValidator DefaultTieredPoolMetricValidator()
    {
        return new TieredPoolMetricValidator(
            new List<ITieredPoolMetricRule>
            {
                new TieredPoolMetricAllowedExternalRefRule(new List<string> { "seasonal" }),
                new TieredPoolMetricRequiredLabelRule("tiered pool metric needs a value"),
                new TieredPoolMetricPinnedGuardRule(1000),
                new TieredPoolMetricCodePrefixRule("REF"),
            });
    }
}
