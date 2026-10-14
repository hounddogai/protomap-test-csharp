namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped access badge draft must pass. Check returns its problems, or none.</summary>
public interface ICappedAccessBadgeDraftRule
{
    List<string> Check(CappedAccessBadgeDraft item);
}

public sealed record CappedAccessBadgeDraftCodePrefixRule(string Prefix) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeDraftRequiredCodeRule(string Message) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeDraftLabelLengthRule(int MaxLength) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeDraftSummaryLengthRule(int MaxLength) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeDraftFteRangeRule(double Min, double Max) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccessBadgeDraftAllowedLabelRule(List<string> Allowed) : ICappedAccessBadgeDraftRule
{
    public List<string> Check(CappedAccessBadgeDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped access badge draft.</summary>
public sealed record CappedAccessBadgeDraftValidator(List<ICappedAccessBadgeDraftRule> Rules)
{
    public List<string> Validate(CappedAccessBadgeDraft item)
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

    public void RequireValid(CappedAccessBadgeDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccessBadgeDraft> items)
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

/// <summary>Functions of capped access badge draft rules.</summary>
public static class CappedAccessBadgeDraftRules
{
    public static CappedAccessBadgeDraftValidator DefaultCappedAccessBadgeDraftValidator()
    {
        return new CappedAccessBadgeDraftValidator(
            new List<ICappedAccessBadgeDraftRule>
            {
                new CappedAccessBadgeDraftCodePrefixRule("X"),
                new CappedAccessBadgeDraftRequiredCodeRule("capped access badge draft needs a value"),
                new CappedAccessBadgeDraftLabelLengthRule(86400),
                new CappedAccessBadgeDraftSummaryLengthRule(120),
                new CappedAccessBadgeDraftFteRangeRule(2.8, 3854.8),
                new CappedAccessBadgeDraftAllowedLabelRule(new List<string>()),
            });
    }
}
