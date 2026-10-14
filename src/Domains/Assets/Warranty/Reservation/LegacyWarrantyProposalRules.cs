namespace Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>A rule a legacy warranty proposal must pass. Check returns its problems, or none.</summary>
public interface ILegacyWarrantyProposalRule
{
    List<string> Check(LegacyWarrantyProposal item);
}

public sealed record LegacyWarrantyProposalCapacityRangeRule(int Min, int Max) : ILegacyWarrantyProposalRule
{
    public List<string> Check(LegacyWarrantyProposal item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyWarrantyProposalRequiredDescriptionRule(string Message) : ILegacyWarrantyProposalRule
{
    public List<string> Check(LegacyWarrantyProposal item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyWarrantyProposalCodeLengthRule(int MaxLength) : ILegacyWarrantyProposalRule
{
    public List<string> Check(LegacyWarrantyProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyWarrantyProposalRequiredTeamRule(string Message) : ILegacyWarrantyProposalRule
{
    public List<string> Check(LegacyWarrantyProposal item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy warranty proposal.</summary>
public sealed record LegacyWarrantyProposalValidator(List<ILegacyWarrantyProposalRule> Rules)
{
    public List<string> Validate(LegacyWarrantyProposal item)
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

    public void RequireValid(LegacyWarrantyProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyWarrantyProposal> items)
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

/// <summary>Functions of legacy warranty proposal rules.</summary>
public static class LegacyWarrantyProposalRules
{
    public static LegacyWarrantyProposalValidator DefaultLegacyWarrantyProposalValidator()
    {
        return new LegacyWarrantyProposalValidator(
            new List<ILegacyWarrantyProposalRule>
            {
                new LegacyWarrantyProposalCapacityRangeRule(64, 86400),
                new LegacyWarrantyProposalRequiredDescriptionRule("legacy warranty proposal needs a value"),
                new LegacyWarrantyProposalCodeLengthRule(255),
                new LegacyWarrantyProposalRequiredTeamRule("legacy warranty proposal needs a value"),
            });
    }
}
