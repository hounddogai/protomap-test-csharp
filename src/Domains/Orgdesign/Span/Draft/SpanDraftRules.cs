namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a span draft must pass. Check returns its problems, or none.</summary>
public interface ISpanDraftRule
{
    List<string> Check(SpanDraft item);
}

public sealed record SpanDraftBatchSizeRangeRule(int Min, int Max) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanDraftRequiredTitleRule(string Message) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanDraftAllowedTitleRule(List<string> Allowed) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SpanDraftJobCodeLengthRule(int MaxLength) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanDraftCurrencyLengthRule(int MaxLength) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanDraftRequiredJobCodeRule(string Message) : ISpanDraftRule
{
    public List<string> Check(SpanDraft item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span draft.</summary>
public sealed record SpanDraftValidator(List<ISpanDraftRule> Rules)
{
    public List<string> Validate(SpanDraft item)
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

    public void RequireValid(SpanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanDraft> items)
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

/// <summary>Functions of span draft rules.</summary>
public static class SpanDraftRules
{
    public static SpanDraftValidator DefaultSpanDraftValidator()
    {
        return new SpanDraftValidator(
            new List<ISpanDraftRule>
            {
                new SpanDraftBatchSizeRangeRule(16, 3600),
                new SpanDraftRequiredTitleRule("span draft needs an entry"),
                new SpanDraftAllowedTitleRule(new List<string> { "regional", "pending", "global" }),
                new SpanDraftJobCodeLengthRule(3600),
                new SpanDraftCurrencyLengthRule(1000),
                new SpanDraftRequiredJobCodeRule("span draft needs an entry"),
            });
    }
}
