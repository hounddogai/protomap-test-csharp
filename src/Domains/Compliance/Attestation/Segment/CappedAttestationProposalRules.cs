namespace Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>A rule a capped attestation proposal must pass. Check returns its problems, or none.</summary>
public interface ICappedAttestationProposalRule
{
    List<string> Check(CappedAttestationProposal item);
}

public sealed record CappedAttestationProposalRequiredTitleRule(string Message) : ICappedAttestationProposalRule
{
    public List<string> Check(CappedAttestationProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedAttestationProposalCapacityRangeRule(int Min, int Max) : ICappedAttestationProposalRule
{
    public List<string> Check(CappedAttestationProposal item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAttestationProposalRequiredLocaleRule(string Message) : ICappedAttestationProposalRule
{
    public List<string> Check(CappedAttestationProposal item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped attestation proposal.</summary>
public sealed record CappedAttestationProposalValidator(List<ICappedAttestationProposalRule> Rules)
{
    public List<string> Validate(CappedAttestationProposal item)
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

    public void RequireValid(CappedAttestationProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAttestationProposal> items)
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

/// <summary>Functions of capped attestation proposal rules.</summary>
public static class CappedAttestationProposalRules
{
    public static CappedAttestationProposalValidator DefaultCappedAttestationProposalValidator()
    {
        return new CappedAttestationProposalValidator(
            new List<ICappedAttestationProposalRule>
            {
                new CappedAttestationProposalRequiredTitleRule("capped attestation proposal needs this field"),
                new CappedAttestationProposalCapacityRangeRule(32, 1000),
                new CappedAttestationProposalRequiredLocaleRule("capped attestation proposal needs a value"),
            });
    }
}
