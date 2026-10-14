namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility draft must pass. Check returns its problems, or none.</summary>
public interface IMobilityDraftRule
{
    List<string> Check(MobilityDraft item);
}

public sealed record MobilityDraftRequiredCodeRule(string Message) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityDraftCodeLengthRule(int MaxLength) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityDraftAllowedUnitRule(List<string> Allowed) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record MobilityDraftRequiredUnitRule(string Message) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityDraftCodePrefixRule(string Prefix) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityDraftRequiredGradeRule(string Message) : IMobilityDraftRule
{
    public List<string> Check(MobilityDraft item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility draft.</summary>
public sealed record MobilityDraftValidator(List<IMobilityDraftRule> Rules)
{
    public List<string> Validate(MobilityDraft item)
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

    public void RequireValid(MobilityDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityDraft> items)
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

/// <summary>Functions of mobility draft rules.</summary>
public static class MobilityDraftRules
{
    public static MobilityDraftValidator DefaultMobilityDraftValidator()
    {
        return new MobilityDraftValidator(
            new List<IMobilityDraftRule>
            {
                new MobilityDraftRequiredCodeRule("mobility draft needs an entry"),
                new MobilityDraftCodeLengthRule(1000),
                new MobilityDraftAllowedUnitRule(new List<string> { "global" }),
                new MobilityDraftRequiredUnitRule("mobility draft needs a value"),
                new MobilityDraftCodePrefixRule("Q"),
                new MobilityDraftRequiredGradeRule("mobility draft needs this field"),
            });
    }
}
