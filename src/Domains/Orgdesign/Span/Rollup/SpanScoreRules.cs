namespace Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>A rule a span score must pass. Check returns its problems, or none.</summary>
public interface ISpanScoreRule
{
    List<string> Check(SpanScore item);
}

public sealed record SpanScoreStartsAtRangeRule(int Min, int Max) : ISpanScoreRule
{
    public List<string> Check(SpanScore item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanScoreRequiredLocationRule(string Message) : ISpanScoreRule
{
    public List<string> Check(SpanScore item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanScoreCodeLengthRule(int MaxLength) : ISpanScoreRule
{
    public List<string> Check(SpanScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanScoreTitleLengthRule(int MaxLength) : ISpanScoreRule
{
    public List<string> Check(SpanScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanScoreChecksumLengthRule(int MaxLength) : ISpanScoreRule
{
    public List<string> Check(SpanScore item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span score.</summary>
public sealed record SpanScoreValidator(List<ISpanScoreRule> Rules)
{
    public List<string> Validate(SpanScore item)
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

    public void RequireValid(SpanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanScore> items)
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

/// <summary>Functions of span score rules.</summary>
public static class SpanScoreRules
{
    public static SpanScoreValidator DefaultSpanScoreValidator()
    {
        return new SpanScoreValidator(
            new List<ISpanScoreRule>
            {
                new SpanScoreStartsAtRangeRule(16, 3600),
                new SpanScoreRequiredLocationRule("span score needs an entry"),
                new SpanScoreCodeLengthRule(86400),
                new SpanScoreTitleLengthRule(255),
                new SpanScoreChecksumLengthRule(80),
            });
    }
}
