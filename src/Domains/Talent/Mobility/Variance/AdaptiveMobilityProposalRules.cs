namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive mobility proposal must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveMobilityProposalRule
{
    List<string> Check(AdaptiveMobilityProposal item);
}

public sealed record AdaptiveMobilityProposalNameLengthRule(int MaxLength) : IAdaptiveMobilityProposalRule
{
    public List<string> Check(AdaptiveMobilityProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMobilityProposalCodeLengthRule(int MaxLength) : IAdaptiveMobilityProposalRule
{
    public List<string> Check(AdaptiveMobilityProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMobilityProposalRequiredCurrencyRule(string Message) : IAdaptiveMobilityProposalRule
{
    public List<string> Check(AdaptiveMobilityProposal item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMobilityProposalAllowedCurrencyRule(List<string> Allowed) : IAdaptiveMobilityProposalRule
{
    public List<string> Check(AdaptiveMobilityProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive mobility proposal.</summary>
public sealed record AdaptiveMobilityProposalValidator(List<IAdaptiveMobilityProposalRule> Rules)
{
    public List<string> Validate(AdaptiveMobilityProposal item)
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

    public void RequireValid(AdaptiveMobilityProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveMobilityProposal> items)
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

/// <summary>Functions of adaptive mobility proposal rules.</summary>
public static class AdaptiveMobilityProposalRules
{
    public static AdaptiveMobilityProposalValidator DefaultAdaptiveMobilityProposalValidator()
    {
        return new AdaptiveMobilityProposalValidator(
            new List<IAdaptiveMobilityProposalRule>
            {
                new AdaptiveMobilityProposalNameLengthRule(16),
                new AdaptiveMobilityProposalCodeLengthRule(86400),
                new AdaptiveMobilityProposalRequiredCurrencyRule("adaptive mobility proposal needs a value"),
                new AdaptiveMobilityProposalAllowedCurrencyRule(new List<string> { "primary" }),
            });
    }
}
