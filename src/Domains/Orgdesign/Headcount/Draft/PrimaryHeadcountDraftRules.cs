namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary headcount draft must pass. Check returns its problems, or none.</summary>
public interface IPrimaryHeadcountDraftRule
{
    List<string> Check(PrimaryHeadcountDraft item);
}

public sealed record PrimaryHeadcountDraftApprovedGuardRule(double Limit) : IPrimaryHeadcountDraftRule
{
    public List<string> Check(PrimaryHeadcountDraft item)
    {
        if (item.Approved && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountDraftAllowedCodeRule(List<string> Allowed) : IPrimaryHeadcountDraftRule
{
    public List<string> Check(PrimaryHeadcountDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountDraftRequiredTitleRule(string Message) : IPrimaryHeadcountDraftRule
{
    public List<string> Check(PrimaryHeadcountDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountDraftRequiredCodeRule(string Message) : IPrimaryHeadcountDraftRule
{
    public List<string> Check(PrimaryHeadcountDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountDraftCodePrefixRule(string Prefix) : IPrimaryHeadcountDraftRule
{
    public List<string> Check(PrimaryHeadcountDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary headcount draft.</summary>
public sealed record PrimaryHeadcountDraftValidator(List<IPrimaryHeadcountDraftRule> Rules)
{
    public List<string> Validate(PrimaryHeadcountDraft item)
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

    public void RequireValid(PrimaryHeadcountDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryHeadcountDraft> items)
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

/// <summary>Functions of primary headcount draft rules.</summary>
public static class PrimaryHeadcountDraftRules
{
    public static PrimaryHeadcountDraftValidator DefaultPrimaryHeadcountDraftValidator()
    {
        return new PrimaryHeadcountDraftValidator(
            new List<IPrimaryHeadcountDraftRule>
            {
                new PrimaryHeadcountDraftApprovedGuardRule(4.5),
                new PrimaryHeadcountDraftAllowedCodeRule(new List<string> { "global", "fallback", "lenient" }),
                new PrimaryHeadcountDraftRequiredTitleRule("primary headcount draft needs this field"),
                new PrimaryHeadcountDraftRequiredCodeRule("primary headcount draft needs an entry"),
                new PrimaryHeadcountDraftCodePrefixRule("Q"),
            });
    }
}
