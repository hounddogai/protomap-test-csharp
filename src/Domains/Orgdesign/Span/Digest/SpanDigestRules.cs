namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span digest must pass. Check returns its problems, or none.</summary>
public interface ISpanDigestRule
{
    List<string> Check(SpanDigest item);
}

public sealed record SpanDigestTitleLengthRule(int MaxLength) : ISpanDigestRule
{
    public List<string> Check(SpanDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanDigestAllowedNameRule(List<string> Allowed) : ISpanDigestRule
{
    public List<string> Check(SpanDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SpanDigestFteRangeRule(double Min, double Max) : ISpanDigestRule
{
    public List<string> Check(SpanDigest item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanDigestNameLengthRule(int MaxLength) : ISpanDigestRule
{
    public List<string> Check(SpanDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span digest.</summary>
public sealed record SpanDigestValidator(List<ISpanDigestRule> Rules)
{
    public List<string> Validate(SpanDigest item)
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

    public void RequireValid(SpanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanDigest> items)
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

/// <summary>Functions of span digest rules.</summary>
public static class SpanDigestRules
{
    public static SpanDigestValidator DefaultSpanDigestValidator()
    {
        return new SpanDigestValidator(
            new List<ISpanDigestRule>
            {
                new SpanDigestTitleLengthRule(64),
                new SpanDigestAllowedNameRule(new List<string>()),
                new SpanDigestFteRangeRule(1.8, 3895.0),
                new SpanDigestNameLengthRule(120),
            });
    }
}
