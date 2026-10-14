namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span notice must pass. Check returns its problems, or none.</summary>
public interface ISpanNoticeRule
{
    List<string> Check(SpanNotice item);
}

public sealed record SpanNoticeNotesLengthRule(int MaxLength) : ISpanNoticeRule
{
    public List<string> Check(SpanNotice item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanNoticeCodePrefixRule(string Prefix) : ISpanNoticeRule
{
    public List<string> Check(SpanNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SpanNoticeRequiredNotesRule(string Message) : ISpanNoticeRule
{
    public List<string> Check(SpanNotice item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanNoticeMaximumRangeRule(double Min, double Max) : ISpanNoticeRule
{
    public List<string> Check(SpanNotice item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanNoticeAllowedTitleRule(List<string> Allowed) : ISpanNoticeRule
{
    public List<string> Check(SpanNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span notice.</summary>
public sealed record SpanNoticeValidator(List<ISpanNoticeRule> Rules)
{
    public List<string> Validate(SpanNotice item)
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

    public void RequireValid(SpanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanNotice> items)
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

/// <summary>Functions of span notice rules.</summary>
public static class SpanNoticeRules
{
    public static SpanNoticeValidator DefaultSpanNoticeValidator()
    {
        return new SpanNoticeValidator(
            new List<ISpanNoticeRule>
            {
                new SpanNoticeNotesLengthRule(3600),
                new SpanNoticeCodePrefixRule("OPS"),
                new SpanNoticeRequiredNotesRule("span notice needs an entry"),
                new SpanNoticeMaximumRangeRule(8.5, 4357.3),
                new SpanNoticeAllowedTitleRule(new List<string> { "default" }),
            });
    }
}
