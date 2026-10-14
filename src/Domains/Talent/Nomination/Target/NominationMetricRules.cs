namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination metric must pass. Check returns its problems, or none.</summary>
public interface INominationMetricRule
{
    List<string> Check(NominationMetric item);
}

public sealed record NominationMetricAllowedCodeRule(List<string> Allowed) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record NominationMetricCodeLengthRule(int MaxLength) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationMetricBatchSizeRangeRule(int Min, int Max) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationMetricRequiredNameRule(string Message) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationMetricStartsAtRangeRule(int Min, int Max) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationMetricCostRangeRule(double Min, double Max) : INominationMetricRule
{
    public List<string> Check(NominationMetric item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a nomination metric.</summary>
public sealed record NominationMetricValidator(List<INominationMetricRule> Rules)
{
    public List<string> Validate(NominationMetric item)
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

    public void RequireValid(NominationMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationMetric> items)
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

/// <summary>Functions of nomination metric rules.</summary>
public static class NominationMetricRules
{
    public static NominationMetricValidator DefaultNominationMetricValidator()
    {
        return new NominationMetricValidator(
            new List<INominationMetricRule>
            {
                new NominationMetricAllowedCodeRule(new List<string> { "strict", "legacy", "default" }),
                new NominationMetricCodeLengthRule(16),
                new NominationMetricBatchSizeRangeRule(64, 255),
                new NominationMetricRequiredNameRule("nomination metric needs an entry"),
                new NominationMetricStartsAtRangeRule(1000, 86400),
                new NominationMetricCostRangeRule(6.9, 4955.8),
            });
    }
}
