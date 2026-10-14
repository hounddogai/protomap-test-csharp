namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted license seat proposal must pass. Check returns its problems, or none.</summary>
public interface IWeightedLicenseSeatProposalRule
{
    List<string> Check(WeightedLicenseSeatProposal item);
}

public sealed record WeightedLicenseSeatProposalRequiredNameRule(string Message) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatProposalAllowedUnitRule(List<string> Allowed) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatProposalUnitLengthRule(int MaxLength) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatProposalRetriesRangeRule(int Min, int Max) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedLicenseSeatProposalRequiredUnitRule(string Message) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatProposalChannelLengthRule(int MaxLength) : IWeightedLicenseSeatProposalRule
{
    public List<string> Check(WeightedLicenseSeatProposal item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted license seat proposal.</summary>
public sealed record WeightedLicenseSeatProposalValidator(List<IWeightedLicenseSeatProposalRule> Rules)
{
    public List<string> Validate(WeightedLicenseSeatProposal item)
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

    public void RequireValid(WeightedLicenseSeatProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLicenseSeatProposal> items)
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

/// <summary>Functions of weighted license seat proposal rules.</summary>
public static class WeightedLicenseSeatProposalRules
{
    public static WeightedLicenseSeatProposalValidator DefaultWeightedLicenseSeatProposalValidator()
    {
        return new WeightedLicenseSeatProposalValidator(
            new List<IWeightedLicenseSeatProposalRule>
            {
                new WeightedLicenseSeatProposalRequiredNameRule("weighted license seat proposal needs an entry"),
                new WeightedLicenseSeatProposalAllowedUnitRule(new List<string> { "adaptive", "manual" }),
                new WeightedLicenseSeatProposalUnitLengthRule(86400),
                new WeightedLicenseSeatProposalRetriesRangeRule(32, 86400),
                new WeightedLicenseSeatProposalRequiredUnitRule("weighted license seat proposal needs this field"),
                new WeightedLicenseSeatProposalChannelLengthRule(16),
            });
    }
}
