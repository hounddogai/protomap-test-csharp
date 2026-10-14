namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>A rule a legacy license seat proposal must pass. Check returns its problems, or none.</summary>
public interface ILegacyLicenseSeatProposalRule
{
    List<string> Check(LegacyLicenseSeatProposal item);
}

public sealed record LegacyLicenseSeatProposalRequiredNameRule(string Message) : ILegacyLicenseSeatProposalRule
{
    public List<string> Check(LegacyLicenseSeatProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatProposalMarginRangeRule(double Min, double Max) : ILegacyLicenseSeatProposalRule
{
    public List<string> Check(LegacyLicenseSeatProposal item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyLicenseSeatProposalCeilingRangeRule(double Min, double Max) : ILegacyLicenseSeatProposalRule
{
    public List<string> Check(LegacyLicenseSeatProposal item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatProposalNameLengthRule(int MaxLength) : ILegacyLicenseSeatProposalRule
{
    public List<string> Check(LegacyLicenseSeatProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyLicenseSeatProposalRequiredCategoryRule(string Message) : ILegacyLicenseSeatProposalRule
{
    public List<string> Check(LegacyLicenseSeatProposal item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy license seat proposal.</summary>
public sealed record LegacyLicenseSeatProposalValidator(List<ILegacyLicenseSeatProposalRule> Rules)
{
    public List<string> Validate(LegacyLicenseSeatProposal item)
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

    public void RequireValid(LegacyLicenseSeatProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyLicenseSeatProposal> items)
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

/// <summary>Functions of legacy license seat proposal rules.</summary>
public static class LegacyLicenseSeatProposalRules
{
    public static LegacyLicenseSeatProposalValidator DefaultLegacyLicenseSeatProposalValidator()
    {
        return new LegacyLicenseSeatProposalValidator(
            new List<ILegacyLicenseSeatProposalRule>
            {
                new LegacyLicenseSeatProposalRequiredNameRule("legacy license seat proposal needs an entry"),
                new LegacyLicenseSeatProposalMarginRangeRule(2.0, 1299.0),
                new LegacyLicenseSeatProposalCeilingRangeRule(0.0, 4756.7),
                new LegacyLicenseSeatProposalNameLengthRule(3600),
                new LegacyLicenseSeatProposalRequiredCategoryRule("legacy license seat proposal needs this field"),
            });
    }
}
