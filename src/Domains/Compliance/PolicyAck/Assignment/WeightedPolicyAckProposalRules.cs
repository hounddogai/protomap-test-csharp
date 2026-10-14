namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

/// <summary>A rule a weighted policy ack proposal must pass. Check returns its problems, or none.</summary>
public interface IWeightedPolicyAckProposalRule
{
    List<string> Check(WeightedPolicyAckProposal item);
}

public sealed record WeightedPolicyAckProposalGraceDaysRangeRule(int Min, int Max) : IWeightedPolicyAckProposalRule
{
    public List<string> Check(WeightedPolicyAckProposal item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPolicyAckProposalWindowDaysRangeRule(int Min, int Max) : IWeightedPolicyAckProposalRule
{
    public List<string> Check(WeightedPolicyAckProposal item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckProposalCurrencyLengthRule(int MaxLength) : IWeightedPolicyAckProposalRule
{
    public List<string> Check(WeightedPolicyAckProposal item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted policy ack proposal.</summary>
public sealed record WeightedPolicyAckProposalValidator(List<IWeightedPolicyAckProposalRule> Rules)
{
    public List<string> Validate(WeightedPolicyAckProposal item)
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

    public void RequireValid(WeightedPolicyAckProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPolicyAckProposal> items)
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

/// <summary>Functions of weighted policy ack proposal rules.</summary>
public static class WeightedPolicyAckProposalRules
{
    public static WeightedPolicyAckProposalValidator DefaultWeightedPolicyAckProposalValidator()
    {
        return new WeightedPolicyAckProposalValidator(
            new List<IWeightedPolicyAckProposalRule>
            {
                new WeightedPolicyAckProposalGraceDaysRangeRule(16, 1000),
                new WeightedPolicyAckProposalWindowDaysRangeRule(16, 255),
                new WeightedPolicyAckProposalCurrencyLengthRule(1000),
            });
    }
}
