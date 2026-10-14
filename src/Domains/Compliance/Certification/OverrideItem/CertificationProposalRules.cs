namespace Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>A rule a certification proposal must pass. Check returns its problems, or none.</summary>
public interface ICertificationProposalRule
{
    List<string> Check(CertificationProposal item);
}

public sealed record CertificationProposalRequiredCodeRule(string Message) : ICertificationProposalRule
{
    public List<string> Check(CertificationProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationProposalCodeLengthRule(int MaxLength) : ICertificationProposalRule
{
    public List<string> Check(CertificationProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationProposalCodePrefixRule(string Prefix) : ICertificationProposalRule
{
    public List<string> Check(CertificationProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationProposalQuantityRangeRule(int Min, int Max) : ICertificationProposalRule
{
    public List<string> Check(CertificationProposal item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification proposal.</summary>
public sealed record CertificationProposalValidator(List<ICertificationProposalRule> Rules)
{
    public List<string> Validate(CertificationProposal item)
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

    public void RequireValid(CertificationProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationProposal> items)
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

/// <summary>Functions of certification proposal rules.</summary>
public static class CertificationProposalRules
{
    public static CertificationProposalValidator DefaultCertificationProposalValidator()
    {
        return new CertificationProposalValidator(
            new List<ICertificationProposalRule>
            {
                new CertificationProposalRequiredCodeRule("certification proposal needs a value"),
                new CertificationProposalCodeLengthRule(1000),
                new CertificationProposalCodePrefixRule("OPS"),
                new CertificationProposalQuantityRangeRule(64, 1000),
            });
    }
}
