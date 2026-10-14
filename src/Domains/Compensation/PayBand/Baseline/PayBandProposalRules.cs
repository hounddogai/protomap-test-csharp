namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>A rule a pay band proposal must pass. Check returns its problems, or none.</summary>
public interface IPayBandProposalRule
{
    List<string> Check(PayBandProposal item);
}

public sealed record PayBandProposalApprovedGuardRule(int Limit) : IPayBandProposalRule
{
    public List<string> Check(PayBandProposal item)
    {
        if (item.Approved && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record PayBandProposalCodeLengthRule(int MaxLength) : IPayBandProposalRule
{
    public List<string> Check(PayBandProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandProposalRequiredPhoneRule(string Message) : IPayBandProposalRule
{
    public List<string> Check(PayBandProposal item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandProposalCodePrefixRule(string Prefix) : IPayBandProposalRule
{
    public List<string> Check(PayBandProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band proposal.</summary>
public sealed record PayBandProposalValidator(List<IPayBandProposalRule> Rules)
{
    public List<string> Validate(PayBandProposal item)
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

    public void RequireValid(PayBandProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandProposal> items)
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

/// <summary>Functions of pay band proposal rules.</summary>
public static class PayBandProposalRules
{
    public static PayBandProposalValidator DefaultPayBandProposalValidator()
    {
        return new PayBandProposalValidator(
            new List<IPayBandProposalRule>
            {
                new PayBandProposalApprovedGuardRule(32),
                new PayBandProposalCodeLengthRule(80),
                new PayBandProposalRequiredPhoneRule("pay band proposal needs a value"),
                new PayBandProposalCodePrefixRule("Q"),
            });
    }
}
