namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg metric must pass. Check returns its problems, or none.</summary>
public interface IReorgMetricRule
{
    List<string> Check(ReorgMetric item);
}

public sealed record ReorgMetricBatchSizeRangeRule(int Min, int Max) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMetricCodePrefixRule(string Prefix) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMetricRequiredLocaleRule(string Message) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgMetricTitleLengthRule(int MaxLength) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMetricAllowedCodeRule(List<string> Allowed) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ReorgMetricRequiredTitleRule(string Message) : IReorgMetricRule
{
    public List<string> Check(ReorgMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg metric.</summary>
public sealed record ReorgMetricValidator(List<IReorgMetricRule> Rules)
{
    public List<string> Validate(ReorgMetric item)
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

    public void RequireValid(ReorgMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgMetric> items)
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

/// <summary>Functions of reorg metric rules.</summary>
public static class ReorgMetricRules
{
    public static ReorgMetricValidator DefaultReorgMetricValidator()
    {
        return new ReorgMetricValidator(
            new List<IReorgMetricRule>
            {
                new ReorgMetricBatchSizeRangeRule(120, 120),
                new ReorgMetricCodePrefixRule("X"),
                new ReorgMetricRequiredLocaleRule("reorg metric needs this field"),
                new ReorgMetricTitleLengthRule(86400),
                new ReorgMetricAllowedCodeRule(new List<string>()),
                new ReorgMetricRequiredTitleRule("reorg metric needs a value"),
            });
    }
}
