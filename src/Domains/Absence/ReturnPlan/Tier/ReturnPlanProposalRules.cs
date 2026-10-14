namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan proposal must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanProposalRule
{
    List<string> Check(ReturnPlanProposal item);
}

public sealed record ReturnPlanProposalCodePrefixRule(string Prefix) : IReturnPlanProposalRule
{
    public List<string> Check(ReturnPlanProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProposalRequiredLabelRule(string Message) : IReturnPlanProposalRule
{
    public List<string> Check(ReturnPlanProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProposalAllowedLocaleRule(List<string> Allowed) : IReturnPlanProposalRule
{
    public List<string> Check(ReturnPlanProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProposalTeamLengthRule(int MaxLength) : IReturnPlanProposalRule
{
    public List<string> Check(ReturnPlanProposal item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProposalLabelLengthRule(int MaxLength) : IReturnPlanProposalRule
{
    public List<string> Check(ReturnPlanProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan proposal.</summary>
public sealed record ReturnPlanProposalValidator(List<IReturnPlanProposalRule> Rules)
{
    public List<string> Validate(ReturnPlanProposal item)
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

    public void RequireValid(ReturnPlanProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanProposal> items)
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

/// <summary>Functions of return plan proposal rules.</summary>
public static class ReturnPlanProposalRules
{
    public static ReturnPlanProposalValidator DefaultReturnPlanProposalValidator()
    {
        return new ReturnPlanProposalValidator(
            new List<IReturnPlanProposalRule>
            {
                new ReturnPlanProposalCodePrefixRule("Q"),
                new ReturnPlanProposalRequiredLabelRule("return plan proposal needs this field"),
                new ReturnPlanProposalAllowedLocaleRule(new List<string> { "global", "default" }),
                new ReturnPlanProposalTeamLengthRule(3600),
                new ReturnPlanProposalLabelLengthRule(64),
            });
    }
}
