namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered skill profile proposal must pass. Check returns its problems, or none.</summary>
public interface ITieredSkillProfileProposalRule
{
    List<string> Check(TieredSkillProfileProposal item);
}

public sealed record TieredSkillProfileProposalRegionLengthRule(int MaxLength) : ITieredSkillProfileProposalRule
{
    public List<string> Check(TieredSkillProfileProposal item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileProposalRequiredLabelRule(string Message) : ITieredSkillProfileProposalRule
{
    public List<string> Check(TieredSkillProfileProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileProposalAllowedNotesRule(List<string> Allowed) : ITieredSkillProfileProposalRule
{
    public List<string> Check(TieredSkillProfileProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered skill profile proposal.</summary>
public sealed record TieredSkillProfileProposalValidator(List<ITieredSkillProfileProposalRule> Rules)
{
    public List<string> Validate(TieredSkillProfileProposal item)
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

    public void RequireValid(TieredSkillProfileProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSkillProfileProposal> items)
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

/// <summary>Functions of tiered skill profile proposal rules.</summary>
public static class TieredSkillProfileProposalRules
{
    public static TieredSkillProfileProposalValidator DefaultTieredSkillProfileProposalValidator()
    {
        return new TieredSkillProfileProposalValidator(
            new List<ITieredSkillProfileProposalRule>
            {
                new TieredSkillProfileProposalRegionLengthRule(255),
                new TieredSkillProfileProposalRequiredLabelRule("tiered skill profile proposal needs a value"),
                new TieredSkillProfileProposalAllowedNotesRule(new List<string> { "weighted", "tiered", "adaptive" }),
            });
    }
}
