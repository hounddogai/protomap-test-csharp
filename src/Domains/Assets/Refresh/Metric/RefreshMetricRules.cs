namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh metric must pass. Check returns its problems, or none.</summary>
public interface IRefreshMetricRule
{
    List<string> Check(RefreshMetric item);
}

public sealed record RefreshMetricAllowedCodeRule(List<string> Allowed) : IRefreshMetricRule
{
    public List<string> Check(RefreshMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMetricRequiredCodeRule(string Message) : IRefreshMetricRule
{
    public List<string> Check(RefreshMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshMetricCodeLengthRule(int MaxLength) : IRefreshMetricRule
{
    public List<string> Check(RefreshMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMetricLocaleLengthRule(int MaxLength) : IRefreshMetricRule
{
    public List<string> Check(RefreshMetric item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh metric.</summary>
public sealed record RefreshMetricValidator(List<IRefreshMetricRule> Rules)
{
    public List<string> Validate(RefreshMetric item)
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

    public void RequireValid(RefreshMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshMetric> items)
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

/// <summary>Functions of refresh metric rules.</summary>
public static class RefreshMetricRules
{
    public static RefreshMetricValidator DefaultRefreshMetricValidator()
    {
        return new RefreshMetricValidator(
            new List<IRefreshMetricRule>
            {
                new RefreshMetricAllowedCodeRule(new List<string> { "batched", "fallback", "manual" }),
                new RefreshMetricRequiredCodeRule("refresh metric needs a value"),
                new RefreshMetricCodeLengthRule(120),
                new RefreshMetricLocaleLengthRule(1000),
            });
    }
}
