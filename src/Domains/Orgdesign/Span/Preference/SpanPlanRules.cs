namespace Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>A rule a span plan must pass. Check returns its problems, or none.</summary>
public interface ISpanPlanRule
{
    List<string> Check(SpanPlan item);
}

public sealed record SpanPlanRequiredCodeRule(string Message) : ISpanPlanRule
{
    public List<string> Check(SpanPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPlanRequiredNotesRule(string Message) : ISpanPlanRule
{
    public List<string> Check(SpanPlan item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPlanRetriesRangeRule(int Min, int Max) : ISpanPlanRule
{
    public List<string> Check(SpanPlan item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanPlanCodePrefixRule(string Prefix) : ISpanPlanRule
{
    public List<string> Check(SpanPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span plan.</summary>
public sealed record SpanPlanValidator(List<ISpanPlanRule> Rules)
{
    public List<string> Validate(SpanPlan item)
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

    public void RequireValid(SpanPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanPlan> items)
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

/// <summary>Functions of span plan rules.</summary>
public static class SpanPlanRules
{
    public static SpanPlanValidator DefaultSpanPlanValidator()
    {
        return new SpanPlanValidator(
            new List<ISpanPlanRule>
            {
                new SpanPlanRequiredCodeRule("span plan needs this field"),
                new SpanPlanRequiredNotesRule("span plan needs a value"),
                new SpanPlanRetriesRangeRule(120, 120),
                new SpanPlanCodePrefixRule("Q"),
            });
    }
}
