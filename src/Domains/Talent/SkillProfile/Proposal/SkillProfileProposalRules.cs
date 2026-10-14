namespace Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>A rule a skill profile proposal must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileProposalRule
{
    List<string> Check(SkillProfileProposal item);
}

public sealed record SkillProfileProposalRequiredLabelRule(string Message) : ISkillProfileProposalRule
{
    public List<string> Check(SkillProfileProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileProposalRequiredCodeRule(string Message) : ISkillProfileProposalRule
{
    public List<string> Check(SkillProfileProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileProposalStartsAtRangeRule(int Min, int Max) : ISkillProfileProposalRule
{
    public List<string> Check(SkillProfileProposal item)
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

public sealed record SkillProfileProposalRequiredCategoryRule(string Message) : ISkillProfileProposalRule
{
    public List<string> Check(SkillProfileProposal item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile proposal.</summary>
public sealed record SkillProfileProposalValidator(List<ISkillProfileProposalRule> Rules)
{
    public List<string> Validate(SkillProfileProposal item)
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

    public void RequireValid(SkillProfileProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileProposal> items)
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

/// <summary>Functions of skill profile proposal rules.</summary>
public static class SkillProfileProposalRules
{
    public static SkillProfileProposalValidator DefaultSkillProfileProposalValidator()
    {
        return new SkillProfileProposalValidator(
            new List<ISkillProfileProposalRule>
            {
                new SkillProfileProposalRequiredLabelRule("skill profile proposal needs this field"),
                new SkillProfileProposalRequiredCodeRule("skill profile proposal needs a value"),
                new SkillProfileProposalStartsAtRangeRule(32, 255),
                new SkillProfileProposalRequiredCategoryRule("skill profile proposal needs a value"),
            });
    }
}
