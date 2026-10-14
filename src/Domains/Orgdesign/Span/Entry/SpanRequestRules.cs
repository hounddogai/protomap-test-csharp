namespace Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>A rule a span request must pass. Check returns its problems, or none.</summary>
public interface ISpanRequestRule
{
    List<string> Check(SpanRequest item);
}

public sealed record SpanRequestPinnedGuardRule(int Limit) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record SpanRequestRequiredCodeRule(string Message) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanRequestRequiredTeamRule(string Message) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanRequestCodeLengthRule(int MaxLength) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanRequestRequiredCategoryRule(string Message) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanRequestRetriesRangeRule(int Min, int Max) : ISpanRequestRule
{
    public List<string> Check(SpanRequest item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span request.</summary>
public sealed record SpanRequestValidator(List<ISpanRequestRule> Rules)
{
    public List<string> Validate(SpanRequest item)
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

    public void RequireValid(SpanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanRequest> items)
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

/// <summary>Functions of span request rules.</summary>
public static class SpanRequestRules
{
    public static SpanRequestValidator DefaultSpanRequestValidator()
    {
        return new SpanRequestValidator(
            new List<ISpanRequestRule>
            {
                new SpanRequestPinnedGuardRule(1000),
                new SpanRequestRequiredCodeRule("span request needs an entry"),
                new SpanRequestRequiredTeamRule("span request needs this field"),
                new SpanRequestCodeLengthRule(16),
                new SpanRequestRequiredCategoryRule("span request needs this field"),
                new SpanRequestRetriesRangeRule(3600, 86400),
            });
    }
}
