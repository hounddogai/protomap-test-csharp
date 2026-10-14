namespace Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>A rule a span claim must pass. Check returns its problems, or none.</summary>
public interface ISpanClaimRule
{
    List<string> Check(SpanClaim item);
}

public sealed record SpanClaimLocationLengthRule(int MaxLength) : ISpanClaimRule
{
    public List<string> Check(SpanClaim item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanClaimRequiredNameRule(string Message) : ISpanClaimRule
{
    public List<string> Check(SpanClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanClaimWeightRangeRule(double Min, double Max) : ISpanClaimRule
{
    public List<string> Check(SpanClaim item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a span claim.</summary>
public sealed record SpanClaimValidator(List<ISpanClaimRule> Rules)
{
    public List<string> Validate(SpanClaim item)
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

    public void RequireValid(SpanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanClaim> items)
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

/// <summary>Functions of span claim rules.</summary>
public static class SpanClaimRules
{
    public static SpanClaimValidator DefaultSpanClaimValidator()
    {
        return new SpanClaimValidator(
            new List<ISpanClaimRule>
            {
                new SpanClaimLocationLengthRule(32),
                new SpanClaimRequiredNameRule("span claim needs a value"),
                new SpanClaimWeightRangeRule(7.1, 3702.7),
            });
    }
}
