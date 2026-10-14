namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>A rule a mobility agreement must pass. Check returns its problems, or none.</summary>
public interface IMobilityAgreementRule
{
    List<string> Check(MobilityAgreement item);
}

public sealed record MobilityAgreementRequiredCodeRule(string Message) : IMobilityAgreementRule
{
    public List<string> Check(MobilityAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityAgreementRequiredSegmentRule(string Message) : IMobilityAgreementRule
{
    public List<string> Check(MobilityAgreement item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityAgreementCodePrefixRule(string Prefix) : IMobilityAgreementRule
{
    public List<string> Check(MobilityAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility agreement.</summary>
public sealed record MobilityAgreementValidator(List<IMobilityAgreementRule> Rules)
{
    public List<string> Validate(MobilityAgreement item)
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

    public void RequireValid(MobilityAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityAgreement> items)
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

/// <summary>Functions of mobility agreement rules.</summary>
public static class MobilityAgreementRules
{
    public static MobilityAgreementValidator DefaultMobilityAgreementValidator()
    {
        return new MobilityAgreementValidator(
            new List<IMobilityAgreementRule>
            {
                new MobilityAgreementRequiredCodeRule("mobility agreement needs an entry"),
                new MobilityAgreementRequiredSegmentRule("mobility agreement needs this field"),
                new MobilityAgreementCodePrefixRule("OPS"),
            });
    }
}
