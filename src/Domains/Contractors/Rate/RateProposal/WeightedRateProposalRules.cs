namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted rate proposal must pass. Check returns its problems, or none.</summary>
public interface IWeightedRateProposalRule
{
    List<string> Check(WeightedRateProposal item);
}

public sealed record WeightedRateProposalAllowedTitleRule(List<string> Allowed) : IWeightedRateProposalRule
{
    public List<string> Check(WeightedRateProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateProposalQuantityRangeRule(int Min, int Max) : IWeightedRateProposalRule
{
    public List<string> Check(WeightedRateProposal item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateProposalArchivedGuardRule(int Limit) : IWeightedRateProposalRule
{
    public List<string> Check(WeightedRateProposal item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateProposalCodePrefixRule(string Prefix) : IWeightedRateProposalRule
{
    public List<string> Check(WeightedRateProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate proposal.</summary>
public sealed record WeightedRateProposalValidator(List<IWeightedRateProposalRule> Rules)
{
    public List<string> Validate(WeightedRateProposal item)
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

    public void RequireValid(WeightedRateProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRateProposal> items)
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

/// <summary>Functions of weighted rate proposal rules.</summary>
public static class WeightedRateProposalRules
{
    public static WeightedRateProposalValidator DefaultWeightedRateProposalValidator()
    {
        return new WeightedRateProposalValidator(
            new List<IWeightedRateProposalRule>
            {
                new WeightedRateProposalAllowedTitleRule(new List<string>()),
                new WeightedRateProposalQuantityRangeRule(32, 86400),
                new WeightedRateProposalArchivedGuardRule(255),
                new WeightedRateProposalCodePrefixRule("Q"),
            });
    }
}
