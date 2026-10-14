namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional span draft must pass. Check returns its problems, or none.</summary>
public interface IRegionalSpanDraftRule
{
    List<string> Check(RegionalSpanDraft item);
}

public sealed record RegionalSpanDraftMaximumRangeRule(double Min, double Max) : IRegionalSpanDraftRule
{
    public List<string> Check(RegionalSpanDraft item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDraftAllowedNameRule(List<string> Allowed) : IRegionalSpanDraftRule
{
    public List<string> Check(RegionalSpanDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDraftRequiredBadgeRule(string Message) : IRegionalSpanDraftRule
{
    public List<string> Check(RegionalSpanDraft item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSpanDraftCodeLengthRule(int MaxLength) : IRegionalSpanDraftRule
{
    public List<string> Check(RegionalSpanDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional span draft.</summary>
public sealed record RegionalSpanDraftValidator(List<IRegionalSpanDraftRule> Rules)
{
    public List<string> Validate(RegionalSpanDraft item)
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

    public void RequireValid(RegionalSpanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSpanDraft> items)
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

/// <summary>Functions of regional span draft rules.</summary>
public static class RegionalSpanDraftRules
{
    public static RegionalSpanDraftValidator DefaultRegionalSpanDraftValidator()
    {
        return new RegionalSpanDraftValidator(
            new List<IRegionalSpanDraftRule>
            {
                new RegionalSpanDraftMaximumRangeRule(4.9, 2941.9),
                new RegionalSpanDraftAllowedNameRule(new List<string> { "tiered", "legacy", "scheduled" }),
                new RegionalSpanDraftRequiredBadgeRule("regional span draft needs this field"),
                new RegionalSpanDraftCodeLengthRule(80),
            });
    }
}
