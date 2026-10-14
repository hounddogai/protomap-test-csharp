namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>A rule a license seat proposal must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatProposalRule
{
    List<string> Check(LicenseSeatProposal item);
}

public sealed record LicenseSeatProposalBatchSizeRangeRule(int Min, int Max) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatProposalRequiredLocationRule(string Message) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProposalRequiredCodeRule(string Message) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProposalLabelLengthRule(int MaxLength) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProposalRequiredLabelRule(string Message) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProposalCodeLengthRule(int MaxLength) : ILicenseSeatProposalRule
{
    public List<string> Check(LicenseSeatProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat proposal.</summary>
public sealed record LicenseSeatProposalValidator(List<ILicenseSeatProposalRule> Rules)
{
    public List<string> Validate(LicenseSeatProposal item)
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

    public void RequireValid(LicenseSeatProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatProposal> items)
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

/// <summary>Functions of license seat proposal rules.</summary>
public static class LicenseSeatProposalRules
{
    public static LicenseSeatProposalValidator DefaultLicenseSeatProposalValidator()
    {
        return new LicenseSeatProposalValidator(
            new List<ILicenseSeatProposalRule>
            {
                new LicenseSeatProposalBatchSizeRangeRule(32, 120),
                new LicenseSeatProposalRequiredLocationRule("license seat proposal needs a value"),
                new LicenseSeatProposalRequiredCodeRule("license seat proposal needs an entry"),
                new LicenseSeatProposalLabelLengthRule(80),
                new LicenseSeatProposalRequiredLabelRule("license seat proposal needs a value"),
                new LicenseSeatProposalCodeLengthRule(32),
            });
    }
}
