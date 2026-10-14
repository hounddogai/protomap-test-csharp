namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate proposal must pass. Check returns its problems, or none.</summary>
public interface IRateProposalRule
{
    List<string> Check(RateProposal item);
}

public sealed record RateProposalCapacityRangeRule(int Min, int Max) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateProposalAllowedTitleRule(List<string> Allowed) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RateProposalCodePrefixRule(string Prefix) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateProposalSalaryBandLengthRule(int MaxLength) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateProposalRequiredSalaryBandRule(string Message) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateProposalCreatedAtRangeRule(int Min, int Max) : IRateProposalRule
{
    public List<string> Check(RateProposal item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate proposal.</summary>
public sealed record RateProposalValidator(List<IRateProposalRule> Rules)
{
    public List<string> Validate(RateProposal item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(RateProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateProposal> items)
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

/// <summary>Functions of rate proposal rules.</summary>
public static class RateProposalRules
{
    public static RateProposalValidator DefaultRateProposalValidator()
    {
        return new RateProposalValidator(
            new List<IRateProposalRule>
            {
                new RateProposalCapacityRangeRule(3600, 3600),
                new RateProposalAllowedTitleRule(new List<string> { "batched", "seasonal" }),
                new RateProposalCodePrefixRule("Q"),
                new RateProposalSalaryBandLengthRule(80),
                new RateProposalRequiredSalaryBandRule("rate proposal needs an entry"),
                new RateProposalCreatedAtRangeRule(80, 120),
            });
    }
}
