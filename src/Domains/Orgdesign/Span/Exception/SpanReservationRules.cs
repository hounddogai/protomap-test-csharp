namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span reservation must pass. Check returns its problems, or none.</summary>
public interface ISpanReservationRule
{
    List<string> Check(SpanReservation item);
}

public sealed record SpanReservationSequenceRangeRule(int Min, int Max) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanReservationDescriptionLengthRule(int MaxLength) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanReservationAllowedDescriptionRule(List<string> Allowed) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
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

public sealed record SpanReservationRequiredBadgeRule(string Message) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanReservationRequiredDescriptionRule(string Message) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanReservationRequiredTitleRule(string Message) : ISpanReservationRule
{
    public List<string> Check(SpanReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span reservation.</summary>
public sealed record SpanReservationValidator(List<ISpanReservationRule> Rules)
{
    public List<string> Validate(SpanReservation item)
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

    public void RequireValid(SpanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanReservation> items)
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

/// <summary>Functions of span reservation rules.</summary>
public static class SpanReservationRules
{
    public static SpanReservationValidator DefaultSpanReservationValidator()
    {
        return new SpanReservationValidator(
            new List<ISpanReservationRule>
            {
                new SpanReservationSequenceRangeRule(120, 3600),
                new SpanReservationDescriptionLengthRule(86400),
                new SpanReservationAllowedDescriptionRule(new List<string> { "lenient" }),
                new SpanReservationRequiredBadgeRule("span reservation needs a value"),
                new SpanReservationRequiredDescriptionRule("span reservation needs an entry"),
                new SpanReservationRequiredTitleRule("span reservation needs this field"),
            });
    }
}
