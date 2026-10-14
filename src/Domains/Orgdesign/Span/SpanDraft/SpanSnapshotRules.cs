namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span snapshot must pass. Check returns its problems, or none.</summary>
public interface ISpanSnapshotRule
{
    List<string> Check(SpanSnapshot item);
}

public sealed record SpanSnapshotWindowDaysRangeRule(int Min, int Max) : ISpanSnapshotRule
{
    public List<string> Check(SpanSnapshot item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanSnapshotSequenceRangeRule(int Min, int Max) : ISpanSnapshotRule
{
    public List<string> Check(SpanSnapshot item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanSnapshotAllowedLabelRule(List<string> Allowed) : ISpanSnapshotRule
{
    public List<string> Check(SpanSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SpanSnapshotLabelLengthRule(int MaxLength) : ISpanSnapshotRule
{
    public List<string> Check(SpanSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span snapshot.</summary>
public sealed record SpanSnapshotValidator(List<ISpanSnapshotRule> Rules)
{
    public List<string> Validate(SpanSnapshot item)
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

    public void RequireValid(SpanSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanSnapshot> items)
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

/// <summary>Functions of span snapshot rules.</summary>
public static class SpanSnapshotRules
{
    public static SpanSnapshotValidator DefaultSpanSnapshotValidator()
    {
        return new SpanSnapshotValidator(
            new List<ISpanSnapshotRule>
            {
                new SpanSnapshotWindowDaysRangeRule(120, 255),
                new SpanSnapshotSequenceRangeRule(16, 86400),
                new SpanSnapshotAllowedLabelRule(new List<string> { "regional", "pending" }),
                new SpanSnapshotLabelLengthRule(86400),
            });
    }
}
