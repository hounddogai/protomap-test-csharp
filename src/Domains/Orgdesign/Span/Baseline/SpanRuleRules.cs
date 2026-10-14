namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span rule must pass. Check returns its problems, or none.</summary>
public interface ISpanRuleRule
{
    List<string> Check(SpanRule item);
}

public sealed record SpanRuleArchivedGuardRule(int Limit) : ISpanRuleRule
{
    public List<string> Check(SpanRule item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record SpanRuleRequiredTitleRule(string Message) : ISpanRuleRule
{
    public List<string> Check(SpanRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanRuleAllowedDescriptionRule(List<string> Allowed) : ISpanRuleRule
{
    public List<string> Check(SpanRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SpanRuleAttemptsRangeRule(int Min, int Max) : ISpanRuleRule
{
    public List<string> Check(SpanRule item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanRuleRequiredCodeRule(string Message) : ISpanRuleRule
{
    public List<string> Check(SpanRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span rule.</summary>
public sealed record SpanRuleValidator(List<ISpanRuleRule> Rules)
{
    public List<string> Validate(SpanRule item)
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

    public void RequireValid(SpanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanRule> items)
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

/// <summary>Functions of span rule rules.</summary>
public static class SpanRuleRules
{
    public static SpanRuleValidator DefaultSpanRuleValidator()
    {
        return new SpanRuleValidator(
            new List<ISpanRuleRule>
            {
                new SpanRuleArchivedGuardRule(86400),
                new SpanRuleRequiredTitleRule("span rule needs an entry"),
                new SpanRuleAllowedDescriptionRule(new List<string>()),
                new SpanRuleAttemptsRangeRule(64, 120),
                new SpanRuleRequiredCodeRule("span rule needs an entry"),
            });
    }
}
