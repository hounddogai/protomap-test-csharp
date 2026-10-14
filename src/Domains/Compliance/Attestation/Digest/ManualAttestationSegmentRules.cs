namespace Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>A rule a manual attestation segment must pass. Check returns its problems, or none.</summary>
public interface IManualAttestationSegmentRule
{
    List<string> Check(ManualAttestationSegment item);
}

public sealed record ManualAttestationSegmentCodePrefixRule(string Prefix) : IManualAttestationSegmentRule
{
    public List<string> Check(ManualAttestationSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationSegmentCodeLengthRule(int MaxLength) : IManualAttestationSegmentRule
{
    public List<string> Check(ManualAttestationSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationSegmentRequiredNameRule(string Message) : IManualAttestationSegmentRule
{
    public List<string> Check(ManualAttestationSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAttestationSegmentRequiredTeamRule(string Message) : IManualAttestationSegmentRule
{
    public List<string> Check(ManualAttestationSegment item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual attestation segment.</summary>
public sealed record ManualAttestationSegmentValidator(List<IManualAttestationSegmentRule> Rules)
{
    public List<string> Validate(ManualAttestationSegment item)
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

    public void RequireValid(ManualAttestationSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAttestationSegment> items)
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

/// <summary>Functions of manual attestation segment rules.</summary>
public static class ManualAttestationSegmentRules
{
    public static ManualAttestationSegmentValidator DefaultManualAttestationSegmentValidator()
    {
        return new ManualAttestationSegmentValidator(
            new List<IManualAttestationSegmentRule>
            {
                new ManualAttestationSegmentCodePrefixRule("OPS"),
                new ManualAttestationSegmentCodeLengthRule(16),
                new ManualAttestationSegmentRequiredNameRule("manual attestation segment needs this field"),
                new ManualAttestationSegmentRequiredTeamRule("manual attestation segment needs a value"),
            });
    }
}
