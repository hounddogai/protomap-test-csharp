namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy skill profile proposal must pass. Check returns its problems, or none.</summary>
public interface ILegacySkillProfileProposalRule
{
    List<string> Check(LegacySkillProfileProposal item);
}

public sealed record LegacySkillProfileProposalEmployeeIdLengthRule(int MaxLength) : ILegacySkillProfileProposalRule
{
    public List<string> Check(LegacySkillProfileProposal item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfileProposalAllowedCodeRule(List<string> Allowed) : ILegacySkillProfileProposalRule
{
    public List<string> Check(LegacySkillProfileProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfileProposalBudgetRangeRule(double Min, double Max) : ILegacySkillProfileProposalRule
{
    public List<string> Check(LegacySkillProfileProposal item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacySkillProfileProposalMaximumRangeRule(double Min, double Max) : ILegacySkillProfileProposalRule
{
    public List<string> Check(LegacySkillProfileProposal item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy skill profile proposal.</summary>
public sealed record LegacySkillProfileProposalValidator(List<ILegacySkillProfileProposalRule> Rules)
{
    public List<string> Validate(LegacySkillProfileProposal item)
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

    public void RequireValid(LegacySkillProfileProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySkillProfileProposal> items)
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

/// <summary>Functions of legacy skill profile proposal rules.</summary>
public static class LegacySkillProfileProposalRules
{
    public static LegacySkillProfileProposalValidator DefaultLegacySkillProfileProposalValidator()
    {
        return new LegacySkillProfileProposalValidator(
            new List<ILegacySkillProfileProposalRule>
            {
                new LegacySkillProfileProposalEmployeeIdLengthRule(1000),
                new LegacySkillProfileProposalAllowedCodeRule(new List<string> { "default", "strict" }),
                new LegacySkillProfileProposalBudgetRangeRule(3.0, 4259.9),
                new LegacySkillProfileProposalMaximumRangeRule(3.5, 1748.6),
            });
    }
}
