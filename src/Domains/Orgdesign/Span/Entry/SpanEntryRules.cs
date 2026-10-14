namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span entry must pass. Check returns its problems, or none.</summary>
public interface ISpanEntryRule
{
    List<string> Check(SpanEntry item);
}

public sealed record SpanEntryNameLengthRule(int MaxLength) : ISpanEntryRule
{
    public List<string> Check(SpanEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanEntryAllowedCurrencyRule(List<string> Allowed) : ISpanEntryRule
{
    public List<string> Check(SpanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record SpanEntryTeamLengthRule(int MaxLength) : ISpanEntryRule
{
    public List<string> Check(SpanEntry item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanEntryCodePrefixRule(string Prefix) : ISpanEntryRule
{
    public List<string> Check(SpanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span entry.</summary>
public sealed record SpanEntryValidator(List<ISpanEntryRule> Rules)
{
    public List<string> Validate(SpanEntry item)
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

    public void RequireValid(SpanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanEntry> items)
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

/// <summary>Functions of span entry rules.</summary>
public static class SpanEntryRules
{
    public static SpanEntryValidator DefaultSpanEntryValidator()
    {
        return new SpanEntryValidator(
            new List<ISpanEntryRule>
            {
                new SpanEntryNameLengthRule(3600),
                new SpanEntryAllowedCurrencyRule(new List<string>()),
                new SpanEntryTeamLengthRule(80),
                new SpanEntryCodePrefixRule("REF"),
            });
    }
}
