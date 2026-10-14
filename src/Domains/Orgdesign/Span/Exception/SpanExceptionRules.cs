namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span exception must pass. Check returns its problems, or none.</summary>
public interface ISpanExceptionRule
{
    List<string> Check(SpanException item);
}

public sealed record SpanExceptionRetriesRangeRule(int Min, int Max) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanExceptionRequiredCodeRule(string Message) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanExceptionAllowedLabelRule(List<string> Allowed) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SpanExceptionLabelLengthRule(int MaxLength) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanExceptionRequiredChannelRule(string Message) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanExceptionChannelLengthRule(int MaxLength) : ISpanExceptionRule
{
    public List<string> Check(SpanException item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span exception.</summary>
public sealed record SpanExceptionValidator(List<ISpanExceptionRule> Rules)
{
    public List<string> Validate(SpanException item)
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

    public void RequireValid(SpanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanException> items)
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

/// <summary>Functions of span exception rules.</summary>
public static class SpanExceptionRules
{
    public static SpanExceptionValidator DefaultSpanExceptionValidator()
    {
        return new SpanExceptionValidator(
            new List<ISpanExceptionRule>
            {
                new SpanExceptionRetriesRangeRule(64, 120),
                new SpanExceptionRequiredCodeRule("span exception needs an entry"),
                new SpanExceptionAllowedLabelRule(new List<string> { "scheduled", "strict", "rolling" }),
                new SpanExceptionLabelLengthRule(64),
                new SpanExceptionRequiredChannelRule("span exception needs a value"),
                new SpanExceptionChannelLengthRule(1000),
            });
    }
}
