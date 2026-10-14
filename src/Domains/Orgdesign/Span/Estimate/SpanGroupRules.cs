namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>A rule a span group must pass. Check returns its problems, or none.</summary>
public interface ISpanGroupRule
{
    List<string> Check(SpanGroup item);
}

public sealed record SpanGroupCeilingRangeRule(double Min, double Max) : ISpanGroupRule
{
    public List<string> Check(SpanGroup item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanGroupCodeLengthRule(int MaxLength) : ISpanGroupRule
{
    public List<string> Check(SpanGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanGroupRequiredSkillRule(string Message) : ISpanGroupRule
{
    public List<string> Check(SpanGroup item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanGroupLabelLengthRule(int MaxLength) : ISpanGroupRule
{
    public List<string> Check(SpanGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanGroupCodePrefixRule(string Prefix) : ISpanGroupRule
{
    public List<string> Check(SpanGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span group.</summary>
public sealed record SpanGroupValidator(List<ISpanGroupRule> Rules)
{
    public List<string> Validate(SpanGroup item)
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

    public void RequireValid(SpanGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanGroup> items)
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

/// <summary>Functions of span group rules.</summary>
public static class SpanGroupRules
{
    public static SpanGroupValidator DefaultSpanGroupValidator()
    {
        return new SpanGroupValidator(
            new List<ISpanGroupRule>
            {
                new SpanGroupCeilingRangeRule(6.3, 3241.0),
                new SpanGroupCodeLengthRule(16),
                new SpanGroupRequiredSkillRule("span group needs this field"),
                new SpanGroupLabelLengthRule(255),
                new SpanGroupCodePrefixRule("OPS"),
            });
    }
}
