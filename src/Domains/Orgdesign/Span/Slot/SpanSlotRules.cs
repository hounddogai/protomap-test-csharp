namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span slot must pass. Check returns its problems, or none.</summary>
public interface ISpanSlotRule
{
    List<string> Check(SpanSlot item);
}

public sealed record SpanSlotCeilingRangeRule(double Min, double Max) : ISpanSlotRule
{
    public List<string> Check(SpanSlot item)
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

public sealed record SpanSlotCodePrefixRule(string Prefix) : ISpanSlotRule
{
    public List<string> Check(SpanSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanSlotEndsAtRangeRule(int Min, int Max) : ISpanSlotRule
{
    public List<string> Check(SpanSlot item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanSlotAttemptsRangeRule(int Min, int Max) : ISpanSlotRule
{
    public List<string> Check(SpanSlot item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanSlotAllowedCodeRule(List<string> Allowed) : ISpanSlotRule
{
    public List<string> Check(SpanSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span slot.</summary>
public sealed record SpanSlotValidator(List<ISpanSlotRule> Rules)
{
    public List<string> Validate(SpanSlot item)
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

    public void RequireValid(SpanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanSlot> items)
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

/// <summary>Functions of span slot rules.</summary>
public static class SpanSlotRules
{
    public static SpanSlotValidator DefaultSpanSlotValidator()
    {
        return new SpanSlotValidator(
            new List<ISpanSlotRule>
            {
                new SpanSlotCeilingRangeRule(3.1, 4669.9),
                new SpanSlotCodePrefixRule("REF"),
                new SpanSlotEndsAtRangeRule(80, 1000),
                new SpanSlotAttemptsRangeRule(64, 64),
                new SpanSlotAllowedCodeRule(new List<string>()),
            });
    }
}
