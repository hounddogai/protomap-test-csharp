namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled asset return proposal must pass. Check returns its problems, or none.</summary>
public interface IScheduledAssetReturnProposalRule
{
    List<string> Check(ScheduledAssetReturnProposal item);
}

public sealed record ScheduledAssetReturnProposalDepartmentLengthRule(int MaxLength) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnProposalRequiredNameRule(string Message) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnProposalRequiredEmployeeIdRule(string Message) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnProposalCodeLengthRule(int MaxLength) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnProposalAllowedRegionRule(List<string> Allowed) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAssetReturnProposalCodePrefixRule(string Prefix) : IScheduledAssetReturnProposalRule
{
    public List<string> Check(ScheduledAssetReturnProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled asset return proposal.</summary>
public sealed record ScheduledAssetReturnProposalValidator(List<IScheduledAssetReturnProposalRule> Rules)
{
    public List<string> Validate(ScheduledAssetReturnProposal item)
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

    public void RequireValid(ScheduledAssetReturnProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAssetReturnProposal> items)
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

/// <summary>Functions of scheduled asset return proposal rules.</summary>
public static class ScheduledAssetReturnProposalRules
{
    public static ScheduledAssetReturnProposalValidator DefaultScheduledAssetReturnProposalValidator()
    {
        return new ScheduledAssetReturnProposalValidator(
            new List<IScheduledAssetReturnProposalRule>
            {
                new ScheduledAssetReturnProposalDepartmentLengthRule(64),
                new ScheduledAssetReturnProposalRequiredNameRule("scheduled asset return proposal needs this field"),
                new ScheduledAssetReturnProposalRequiredEmployeeIdRule(
                    "scheduled asset return proposal needs a value"),
                new ScheduledAssetReturnProposalCodeLengthRule(64),
                new ScheduledAssetReturnProposalAllowedRegionRule(new List<string> { "scheduled" }),
                new ScheduledAssetReturnProposalCodePrefixRule("REF"),
            });
    }
}
