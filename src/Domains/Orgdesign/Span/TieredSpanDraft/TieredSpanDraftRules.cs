namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>A rule a tiered span draft must pass. Check returns its problems, or none.</summary>
public interface ITieredSpanDraftRule
{
    List<string> Check(TieredSpanDraft item);
}

public sealed record TieredSpanDraftRequiredLabelRule(string Message) : ITieredSpanDraftRule
{
    public List<string> Check(TieredSpanDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSpanDraftRequiredCodeRule(string Message) : ITieredSpanDraftRule
{
    public List<string> Check(TieredSpanDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSpanDraftTitleLengthRule(int MaxLength) : ITieredSpanDraftRule
{
    public List<string> Check(TieredSpanDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered span draft.</summary>
public sealed record TieredSpanDraftValidator(List<ITieredSpanDraftRule> Rules)
{
    public List<string> Validate(TieredSpanDraft item)
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

    public void RequireValid(TieredSpanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSpanDraft> items)
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

/// <summary>Functions of tiered span draft rules.</summary>
public static class TieredSpanDraftRules
{
    public static TieredSpanDraftValidator DefaultTieredSpanDraftValidator()
    {
        return new TieredSpanDraftValidator(
            new List<ITieredSpanDraftRule>
            {
                new TieredSpanDraftRequiredLabelRule("tiered span draft needs an entry"),
                new TieredSpanDraftRequiredCodeRule("tiered span draft needs this field"),
                new TieredSpanDraftTitleLengthRule(86400),
            });
    }
}
