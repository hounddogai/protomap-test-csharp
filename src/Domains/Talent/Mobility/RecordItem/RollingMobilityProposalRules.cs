namespace Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>A rule a rolling mobility proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingMobilityProposalRule
{
    List<string> Check(RollingMobilityProposal item);
}

public sealed record RollingMobilityProposalSourceLengthRule(int MaxLength) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityProposalRequiredLabelRule(string Message) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityProposalRequiredCodeRule(string Message) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityProposalCodeLengthRule(int MaxLength) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityProposalCodePrefixRule(string Prefix) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityProposalExpiresAtRangeRule(int Min, int Max) : IRollingMobilityProposalRule
{
    public List<string> Check(RollingMobilityProposal item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling mobility proposal.</summary>
public sealed record RollingMobilityProposalValidator(List<IRollingMobilityProposalRule> Rules)
{
    public List<string> Validate(RollingMobilityProposal item)
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

    public void RequireValid(RollingMobilityProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMobilityProposal> items)
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

/// <summary>Functions of rolling mobility proposal rules.</summary>
public static class RollingMobilityProposalRules
{
    public static RollingMobilityProposalValidator DefaultRollingMobilityProposalValidator()
    {
        return new RollingMobilityProposalValidator(
            new List<IRollingMobilityProposalRule>
            {
                new RollingMobilityProposalSourceLengthRule(86400),
                new RollingMobilityProposalRequiredLabelRule("rolling mobility proposal needs an entry"),
                new RollingMobilityProposalRequiredCodeRule("rolling mobility proposal needs this field"),
                new RollingMobilityProposalCodeLengthRule(16),
                new RollingMobilityProposalCodePrefixRule("OPS"),
                new RollingMobilityProposalExpiresAtRangeRule(255, 86400),
            });
    }
}
