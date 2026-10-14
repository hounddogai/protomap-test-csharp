namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run metric must pass. Check returns its problems, or none.</summary>
public interface IPayRunMetricRule
{
    List<string> Check(PayRunMetric item);
}

public sealed record PayRunMetricTimezoneLengthRule(int MaxLength) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMetricTitleLengthRule(int MaxLength) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMetricChannelLengthRule(int MaxLength) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMetricAllowedChannelRule(List<string> Allowed) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMetricCodePrefixRule(string Prefix) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMetricCodeLengthRule(int MaxLength) : IPayRunMetricRule
{
    public List<string> Check(PayRunMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run metric.</summary>
public sealed record PayRunMetricValidator(List<IPayRunMetricRule> Rules)
{
    public List<string> Validate(PayRunMetric item)
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

    public void RequireValid(PayRunMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunMetric> items)
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

/// <summary>Functions of pay run metric rules.</summary>
public static class PayRunMetricRules
{
    public static PayRunMetricValidator DefaultPayRunMetricValidator()
    {
        return new PayRunMetricValidator(
            new List<IPayRunMetricRule>
            {
                new PayRunMetricTimezoneLengthRule(120),
                new PayRunMetricTitleLengthRule(80),
                new PayRunMetricChannelLengthRule(255),
                new PayRunMetricAllowedChannelRule(new List<string> { "primary", "rolling", "primary" }),
                new PayRunMetricCodePrefixRule("X"),
                new PayRunMetricCodeLengthRule(120),
            });
    }
}
