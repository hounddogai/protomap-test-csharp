namespace Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>A rule a strict desk proposal must pass. Check returns its problems, or none.</summary>
public interface IStrictDeskProposalRule
{
    List<string> Check(StrictDeskProposal item);
}

public sealed record StrictDeskProposalRateRangeRule(double Min, double Max) : IStrictDeskProposalRule
{
    public List<string> Check(StrictDeskProposal item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictDeskProposalWindowDaysRangeRule(int Min, int Max) : IStrictDeskProposalRule
{
    public List<string> Check(StrictDeskProposal item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeskProposalWeightRangeRule(double Min, double Max) : IStrictDeskProposalRule
{
    public List<string> Check(StrictDeskProposal item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeskProposalExternalRefLengthRule(int MaxLength) : IStrictDeskProposalRule
{
    public List<string> Check(StrictDeskProposal item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict desk proposal.</summary>
public sealed record StrictDeskProposalValidator(List<IStrictDeskProposalRule> Rules)
{
    public List<string> Validate(StrictDeskProposal item)
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

    public void RequireValid(StrictDeskProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeskProposal> items)
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

/// <summary>Functions of strict desk proposal rules.</summary>
public static class StrictDeskProposalRules
{
    public static StrictDeskProposalValidator DefaultStrictDeskProposalValidator()
    {
        return new StrictDeskProposalValidator(
            new List<IStrictDeskProposalRule>
            {
                new StrictDeskProposalRateRangeRule(4.8, 4914.0),
                new StrictDeskProposalWindowDaysRangeRule(32, 32),
                new StrictDeskProposalWeightRangeRule(2.6, 679.2),
                new StrictDeskProposalExternalRefLengthRule(120),
            });
    }
}
