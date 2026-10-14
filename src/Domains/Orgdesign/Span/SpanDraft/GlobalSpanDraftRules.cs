namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global span draft must pass. Check returns its problems, or none.</summary>
public interface IGlobalSpanDraftRule
{
    List<string> Check(GlobalSpanDraft item);
}

public sealed record GlobalSpanDraftBadgeLengthRule(int MaxLength) : IGlobalSpanDraftRule
{
    public List<string> Check(GlobalSpanDraft item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanDraftCodeLengthRule(int MaxLength) : IGlobalSpanDraftRule
{
    public List<string> Check(GlobalSpanDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanDraftRequiredTitleRule(string Message) : IGlobalSpanDraftRule
{
    public List<string> Check(GlobalSpanDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanDraftAllowedTitleRule(List<string> Allowed) : IGlobalSpanDraftRule
{
    public List<string> Check(GlobalSpanDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSpanDraftCodePrefixRule(string Prefix) : IGlobalSpanDraftRule
{
    public List<string> Check(GlobalSpanDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global span draft.</summary>
public sealed record GlobalSpanDraftValidator(List<IGlobalSpanDraftRule> Rules)
{
    public List<string> Validate(GlobalSpanDraft item)
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

    public void RequireValid(GlobalSpanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalSpanDraft> items)
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

/// <summary>Functions of global span draft rules.</summary>
public static class GlobalSpanDraftRules
{
    public static GlobalSpanDraftValidator DefaultGlobalSpanDraftValidator()
    {
        return new GlobalSpanDraftValidator(
            new List<IGlobalSpanDraftRule>
            {
                new GlobalSpanDraftBadgeLengthRule(16),
                new GlobalSpanDraftCodeLengthRule(120),
                new GlobalSpanDraftRequiredTitleRule("global span draft needs an entry"),
                new GlobalSpanDraftAllowedTitleRule(new List<string> { "legacy", "regional", "lenient" }),
                new GlobalSpanDraftCodePrefixRule("OPS"),
            });
    }
}
