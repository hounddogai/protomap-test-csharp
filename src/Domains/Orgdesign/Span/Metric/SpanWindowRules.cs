namespace Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>A rule a span window must pass. Check returns its problems, or none.</summary>
public interface ISpanWindowRule
{
    List<string> Check(SpanWindow item);
}

public sealed record SpanWindowRequiredCodeRule(string Message) : ISpanWindowRule
{
    public List<string> Check(SpanWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanWindowCountryLengthRule(int MaxLength) : ISpanWindowRule
{
    public List<string> Check(SpanWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanWindowRetriesRangeRule(int Min, int Max) : ISpanWindowRule
{
    public List<string> Check(SpanWindow item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a span window.</summary>
public sealed record SpanWindowValidator(List<ISpanWindowRule> Rules)
{
    public List<string> Validate(SpanWindow item)
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

    public void RequireValid(SpanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanWindow> items)
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

/// <summary>Functions of span window rules.</summary>
public static class SpanWindowRules
{
    public static SpanWindowValidator DefaultSpanWindowValidator()
    {
        return new SpanWindowValidator(
            new List<ISpanWindowRule>
            {
                new SpanWindowRequiredCodeRule("span window needs a value"),
                new SpanWindowCountryLengthRule(32),
                new SpanWindowRetriesRangeRule(80, 255),
            });
    }
}
