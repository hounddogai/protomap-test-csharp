namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>A rule a mobility proposal must pass. Check returns its problems, or none.</summary>
public interface IMobilityProposalRule
{
    List<string> Check(MobilityProposal item);
}

public sealed record MobilityProposalSourceLengthRule(int MaxLength) : IMobilityProposalRule
{
    public List<string> Check(MobilityProposal item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityProposalRequiredCodeRule(string Message) : IMobilityProposalRule
{
    public List<string> Check(MobilityProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityProposalNameLengthRule(int MaxLength) : IMobilityProposalRule
{
    public List<string> Check(MobilityProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityProposalRatioRangeRule(double Min, double Max) : IMobilityProposalRule
{
    public List<string> Check(MobilityProposal item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityProposalRequiredTitleRule(string Message) : IMobilityProposalRule
{
    public List<string> Check(MobilityProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility proposal.</summary>
public sealed record MobilityProposalValidator(List<IMobilityProposalRule> Rules)
{
    public List<string> Validate(MobilityProposal item)
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

    public void RequireValid(MobilityProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityProposal> items)
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

/// <summary>Functions of mobility proposal rules.</summary>
public static class MobilityProposalRules
{
    public static MobilityProposalValidator DefaultMobilityProposalValidator()
    {
        return new MobilityProposalValidator(
            new List<IMobilityProposalRule>
            {
                new MobilityProposalSourceLengthRule(32),
                new MobilityProposalRequiredCodeRule("mobility proposal needs a value"),
                new MobilityProposalNameLengthRule(1000),
                new MobilityProposalRatioRangeRule(4.7, 4855.1),
                new MobilityProposalRequiredTitleRule("mobility proposal needs a value"),
            });
    }
}
