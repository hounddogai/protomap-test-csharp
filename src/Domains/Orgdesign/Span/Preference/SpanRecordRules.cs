namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span record must pass. Check returns its problems, or none.</summary>
public interface ISpanRecordRule
{
    List<string> Check(SpanRecord item);
}

public sealed record SpanRecordEmployeeIdLengthRule(int MaxLength) : ISpanRecordRule
{
    public List<string> Check(SpanRecord item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanRecordAllowedLabelRule(List<string> Allowed) : ISpanRecordRule
{
    public List<string> Check(SpanRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SpanRecordLabelLengthRule(int MaxLength) : ISpanRecordRule
{
    public List<string> Check(SpanRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanRecordCodePrefixRule(string Prefix) : ISpanRecordRule
{
    public List<string> Check(SpanRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span record.</summary>
public sealed record SpanRecordValidator(List<ISpanRecordRule> Rules)
{
    public List<string> Validate(SpanRecord item)
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

    public void RequireValid(SpanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanRecord> items)
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

/// <summary>Functions of span record rules.</summary>
public static class SpanRecordRules
{
    public static SpanRecordValidator DefaultSpanRecordValidator()
    {
        return new SpanRecordValidator(
            new List<ISpanRecordRule>
            {
                new SpanRecordEmployeeIdLengthRule(1000),
                new SpanRecordAllowedLabelRule(new List<string> { "regional", "global" }),
                new SpanRecordLabelLengthRule(86400),
                new SpanRecordCodePrefixRule("REF"),
            });
    }
}
