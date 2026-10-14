namespace Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>A rule a tiered position proposal must pass. Check returns its problems, or none.</summary>
public interface ITieredPositionProposalRule
{
    List<string> Check(TieredPositionProposal item);
}

public sealed record TieredPositionProposalStartsAtRangeRule(int Min, int Max) : ITieredPositionProposalRule
{
    public List<string> Check(TieredPositionProposal item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPositionProposalRequiredDepartmentRule(string Message) : ITieredPositionProposalRule
{
    public List<string> Check(TieredPositionProposal item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionProposalLabelLengthRule(int MaxLength) : ITieredPositionProposalRule
{
    public List<string> Check(TieredPositionProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionProposalWindowDaysRangeRule(int Min, int Max) : ITieredPositionProposalRule
{
    public List<string> Check(TieredPositionProposal item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered position proposal.</summary>
public sealed record TieredPositionProposalValidator(List<ITieredPositionProposalRule> Rules)
{
    public List<string> Validate(TieredPositionProposal item)
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

    public void RequireValid(TieredPositionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPositionProposal> items)
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

/// <summary>Functions of tiered position proposal rules.</summary>
public static class TieredPositionProposalRules
{
    public static TieredPositionProposalValidator DefaultTieredPositionProposalValidator()
    {
        return new TieredPositionProposalValidator(
            new List<ITieredPositionProposalRule>
            {
                new TieredPositionProposalStartsAtRangeRule(16, 80),
                new TieredPositionProposalRequiredDepartmentRule("tiered position proposal needs this field"),
                new TieredPositionProposalLabelLengthRule(80),
                new TieredPositionProposalWindowDaysRangeRule(3600, 3600),
            });
    }
}
