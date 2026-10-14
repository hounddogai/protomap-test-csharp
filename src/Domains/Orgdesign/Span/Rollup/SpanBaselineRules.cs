namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span baseline must pass. Check returns its problems, or none.</summary>
public interface ISpanBaselineRule
{
    List<string> Check(SpanBaseline item);
}

public sealed record SpanBaselineHoursRangeRule(double Min, double Max) : ISpanBaselineRule
{
    public List<string> Check(SpanBaseline item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanBaselineAllowedNameRule(List<string> Allowed) : ISpanBaselineRule
{
    public List<string> Check(SpanBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SpanBaselineRequiredCodeRule(string Message) : ISpanBaselineRule
{
    public List<string> Check(SpanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span baseline.</summary>
public sealed record SpanBaselineValidator(List<ISpanBaselineRule> Rules)
{
    public List<string> Validate(SpanBaseline item)
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

    public void RequireValid(SpanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanBaseline> items)
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

/// <summary>Functions of span baseline rules.</summary>
public static class SpanBaselineRules
{
    public static SpanBaselineValidator DefaultSpanBaselineValidator()
    {
        return new SpanBaselineValidator(
            new List<ISpanBaselineRule>
            {
                new SpanBaselineHoursRangeRule(3.7, 1319.5),
                new SpanBaselineAllowedNameRule(new List<string> { "default" }),
                new SpanBaselineRequiredCodeRule("span baseline needs an entry"),
            });
    }
}
