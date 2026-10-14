namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal access badge draft must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccessBadgeDraftRule
{
    List<string> Check(SeasonalAccessBadgeDraft item);
}

public sealed record SeasonalAccessBadgeDraftCodeLengthRule(int MaxLength) : ISeasonalAccessBadgeDraftRule
{
    public List<string> Check(SeasonalAccessBadgeDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeDraftRequiredTitleRule(string Message) : ISeasonalAccessBadgeDraftRule
{
    public List<string> Check(SeasonalAccessBadgeDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeDraftAllowedChecksumRule(List<string> Allowed) : ISeasonalAccessBadgeDraftRule
{
    public List<string> Check(SeasonalAccessBadgeDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccessBadgeDraftCodePrefixRule(string Prefix) : ISeasonalAccessBadgeDraftRule
{
    public List<string> Check(SeasonalAccessBadgeDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal access badge draft.</summary>
public sealed record SeasonalAccessBadgeDraftValidator(List<ISeasonalAccessBadgeDraftRule> Rules)
{
    public List<string> Validate(SeasonalAccessBadgeDraft item)
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

    public void RequireValid(SeasonalAccessBadgeDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccessBadgeDraft> items)
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

/// <summary>Functions of seasonal access badge draft rules.</summary>
public static class SeasonalAccessBadgeDraftRules
{
    public static SeasonalAccessBadgeDraftValidator DefaultSeasonalAccessBadgeDraftValidator()
    {
        return new SeasonalAccessBadgeDraftValidator(
            new List<ISeasonalAccessBadgeDraftRule>
            {
                new SeasonalAccessBadgeDraftCodeLengthRule(80),
                new SeasonalAccessBadgeDraftRequiredTitleRule("seasonal access badge draft needs this field"),
                new SeasonalAccessBadgeDraftAllowedChecksumRule(new List<string> { "capped", "primary", "lenient" }),
                new SeasonalAccessBadgeDraftCodePrefixRule("Q"),
            });
    }
}
