namespace Acme.Workforce.Talent.Nomination.Score;

/// <summary>A rule a nomination claim must pass. Check returns its problems, or none.</summary>
public interface INominationClaimRule
{
    List<string> Check(NominationClaim item);
}

public sealed record NominationClaimRequiredLocationRule(string Message) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationClaimTitleLengthRule(int MaxLength) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationClaimRequiredTitleRule(string Message) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationClaimLocationLengthRule(int MaxLength) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationClaimCodePrefixRule(string Prefix) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationClaimRequiredCodeRule(string Message) : INominationClaimRule
{
    public List<string> Check(NominationClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination claim.</summary>
public sealed record NominationClaimValidator(List<INominationClaimRule> Rules)
{
    public List<string> Validate(NominationClaim item)
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

    public void RequireValid(NominationClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationClaim> items)
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

/// <summary>Functions of nomination claim rules.</summary>
public static class NominationClaimRules
{
    public static NominationClaimValidator DefaultNominationClaimValidator()
    {
        return new NominationClaimValidator(
            new List<INominationClaimRule>
            {
                new NominationClaimRequiredLocationRule("nomination claim needs an entry"),
                new NominationClaimTitleLengthRule(80),
                new NominationClaimRequiredTitleRule("nomination claim needs a value"),
                new NominationClaimLocationLengthRule(64),
                new NominationClaimCodePrefixRule("OPS"),
                new NominationClaimRequiredCodeRule("nomination claim needs this field"),
            });
    }
}
