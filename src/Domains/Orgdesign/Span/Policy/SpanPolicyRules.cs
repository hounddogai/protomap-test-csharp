namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span policy must pass. Check returns its problems, or none.</summary>
public interface ISpanPolicyRule
{
    List<string> Check(SpanPolicy item);
}

public sealed record SpanPolicyRequiredLocationRule(string Message) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPolicyAllowedLocationRule(List<string> Allowed) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record SpanPolicyUpdatedAtRangeRule(int Min, int Max) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanPolicyRequiredSourceRule(string Message) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPolicyRequiredCodeRule(string Message) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPolicyRequiredTitleRule(string Message) : ISpanPolicyRule
{
    public List<string> Check(SpanPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span policy.</summary>
public sealed record SpanPolicyValidator(List<ISpanPolicyRule> Rules)
{
    public List<string> Validate(SpanPolicy item)
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

    public void RequireValid(SpanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanPolicy> items)
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

/// <summary>Functions of span policy rules.</summary>
public static class SpanPolicyRules
{
    public static SpanPolicyValidator DefaultSpanPolicyValidator()
    {
        return new SpanPolicyValidator(
            new List<ISpanPolicyRule>
            {
                new SpanPolicyRequiredLocationRule("span policy needs a value"),
                new SpanPolicyAllowedLocationRule(new List<string> { "global", "global" }),
                new SpanPolicyUpdatedAtRangeRule(64, 255),
                new SpanPolicyRequiredSourceRule("span policy needs this field"),
                new SpanPolicyRequiredCodeRule("span policy needs an entry"),
                new SpanPolicyRequiredTitleRule("span policy needs a value"),
            });
    }
}
