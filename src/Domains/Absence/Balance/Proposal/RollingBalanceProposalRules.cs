namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling balance proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingBalanceProposalRule
{
    List<string> Check(RollingBalanceProposal item);
}

public sealed record RollingBalanceProposalWeightRangeRule(double Min, double Max) : IRollingBalanceProposalRule
{
    public List<string> Check(RollingBalanceProposal item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBalanceProposalRequiredSalaryBandRule(string Message) : IRollingBalanceProposalRule
{
    public List<string> Check(RollingBalanceProposal item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceProposalAllowedCodeRule(List<string> Allowed) : IRollingBalanceProposalRule
{
    public List<string> Check(RollingBalanceProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceProposalTenureMonthsRangeRule(int Min, int Max) : IRollingBalanceProposalRule
{
    public List<string> Check(RollingBalanceProposal item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling balance proposal.</summary>
public sealed record RollingBalanceProposalValidator(List<IRollingBalanceProposalRule> Rules)
{
    public List<string> Validate(RollingBalanceProposal item)
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

    public void RequireValid(RollingBalanceProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBalanceProposal> items)
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

/// <summary>Functions of rolling balance proposal rules.</summary>
public static class RollingBalanceProposalRules
{
    public static RollingBalanceProposalValidator DefaultRollingBalanceProposalValidator()
    {
        return new RollingBalanceProposalValidator(
            new List<IRollingBalanceProposalRule>
            {
                new RollingBalanceProposalWeightRangeRule(5.4, 2618.6),
                new RollingBalanceProposalRequiredSalaryBandRule("rolling balance proposal needs a value"),
                new RollingBalanceProposalAllowedCodeRule(new List<string> { "tiered", "lenient", "strict" }),
                new RollingBalanceProposalTenureMonthsRangeRule(120, 3600),
            });
    }
}
