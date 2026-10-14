namespace Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>A rule a span override must pass. Check returns its problems, or none.</summary>
public interface ISpanOverrideRule
{
    List<string> Check(SpanOverride item);
}

public sealed record SpanOverrideRequiredSourceRule(string Message) : ISpanOverrideRule
{
    public List<string> Check(SpanOverride item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanOverrideRequiredLabelRule(string Message) : ISpanOverrideRule
{
    public List<string> Check(SpanOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanOverrideEmployeeIdLengthRule(int MaxLength) : ISpanOverrideRule
{
    public List<string> Check(SpanOverride item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span override.</summary>
public sealed record SpanOverrideValidator(List<ISpanOverrideRule> Rules)
{
    public List<string> Validate(SpanOverride item)
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

    public void RequireValid(SpanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanOverride> items)
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

/// <summary>Functions of span override rules.</summary>
public static class SpanOverrideRules
{
    public static SpanOverrideValidator DefaultSpanOverrideValidator()
    {
        return new SpanOverrideValidator(
            new List<ISpanOverrideRule>
            {
                new SpanOverrideRequiredSourceRule("span override needs a value"),
                new SpanOverrideRequiredLabelRule("span override needs this field"),
                new SpanOverrideEmployeeIdLengthRule(80),
            });
    }
}
