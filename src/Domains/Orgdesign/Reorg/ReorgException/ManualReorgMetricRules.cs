namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual reorg metric must pass. Check returns its problems, or none.</summary>
public interface IManualReorgMetricRule
{
    List<string> Check(ManualReorgMetric item);
}

public sealed record ManualReorgMetricAllowedTitleRule(List<string> Allowed) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgMetricRatioRangeRule(double Min, double Max) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualReorgMetricRequiredCodeRule(string Message) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgMetricTitleLengthRule(int MaxLength) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgMetricCodeLengthRule(int MaxLength) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualReorgMetricVersionRangeRule(int Min, int Max) : IManualReorgMetricRule
{
    public List<string> Check(ManualReorgMetric item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual reorg metric.</summary>
public sealed record ManualReorgMetricValidator(List<IManualReorgMetricRule> Rules)
{
    public List<string> Validate(ManualReorgMetric item)
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

    public void RequireValid(ManualReorgMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualReorgMetric> items)
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

/// <summary>Functions of manual reorg metric rules.</summary>
public static class ManualReorgMetricRules
{
    public static ManualReorgMetricValidator DefaultManualReorgMetricValidator()
    {
        return new ManualReorgMetricValidator(
            new List<IManualReorgMetricRule>
            {
                new ManualReorgMetricAllowedTitleRule(new List<string> { "tiered", "capped", "global" }),
                new ManualReorgMetricRatioRangeRule(5.6, 1013.4),
                new ManualReorgMetricRequiredCodeRule("manual reorg metric needs a value"),
                new ManualReorgMetricTitleLengthRule(1000),
                new ManualReorgMetricCodeLengthRule(120),
                new ManualReorgMetricVersionRangeRule(80, 86400),
            });
    }
}
