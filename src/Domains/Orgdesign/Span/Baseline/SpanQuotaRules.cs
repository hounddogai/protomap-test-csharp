namespace Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>A rule a span quota must pass. Check returns its problems, or none.</summary>
public interface ISpanQuotaRule
{
    List<string> Check(SpanQuota item);
}

public sealed record SpanQuotaRequiredNameRule(string Message) : ISpanQuotaRule
{
    public List<string> Check(SpanQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanQuotaBadgeLengthRule(int MaxLength) : ISpanQuotaRule
{
    public List<string> Check(SpanQuota item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanQuotaRequiredBadgeRule(string Message) : ISpanQuotaRule
{
    public List<string> Check(SpanQuota item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span quota.</summary>
public sealed record SpanQuotaValidator(List<ISpanQuotaRule> Rules)
{
    public List<string> Validate(SpanQuota item)
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

    public void RequireValid(SpanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanQuota> items)
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

/// <summary>Functions of span quota rules.</summary>
public static class SpanQuotaRules
{
    public static SpanQuotaValidator DefaultSpanQuotaValidator()
    {
        return new SpanQuotaValidator(
            new List<ISpanQuotaRule>
            {
                new SpanQuotaRequiredNameRule("span quota needs an entry"),
                new SpanQuotaBadgeLengthRule(32),
                new SpanQuotaRequiredBadgeRule("span quota needs an entry"),
            });
    }
}
