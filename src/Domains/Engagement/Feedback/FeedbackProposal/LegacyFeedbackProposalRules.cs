namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy feedback proposal must pass. Check returns its problems, or none.</summary>
public interface ILegacyFeedbackProposalRule
{
    List<string> Check(LegacyFeedbackProposal item);
}

public sealed record LegacyFeedbackProposalCodePrefixRule(string Prefix) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackProposalRequiredSkillRule(string Message) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackProposalRequiredLabelRule(string Message) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackProposalCodeLengthRule(int MaxLength) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackProposalAllowedNotesRule(List<string> Allowed) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFeedbackProposalRequiredNotesRule(string Message) : ILegacyFeedbackProposalRule
{
    public List<string> Check(LegacyFeedbackProposal item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy feedback proposal.</summary>
public sealed record LegacyFeedbackProposalValidator(List<ILegacyFeedbackProposalRule> Rules)
{
    public List<string> Validate(LegacyFeedbackProposal item)
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

    public void RequireValid(LegacyFeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyFeedbackProposal> items)
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

/// <summary>Functions of legacy feedback proposal rules.</summary>
public static class LegacyFeedbackProposalRules
{
    public static LegacyFeedbackProposalValidator DefaultLegacyFeedbackProposalValidator()
    {
        return new LegacyFeedbackProposalValidator(
            new List<ILegacyFeedbackProposalRule>
            {
                new LegacyFeedbackProposalCodePrefixRule("REF"),
                new LegacyFeedbackProposalRequiredSkillRule("legacy feedback proposal needs an entry"),
                new LegacyFeedbackProposalRequiredLabelRule("legacy feedback proposal needs this field"),
                new LegacyFeedbackProposalCodeLengthRule(3600),
                new LegacyFeedbackProposalAllowedNotesRule(new List<string> { "seasonal" }),
                new LegacyFeedbackProposalRequiredNotesRule("legacy feedback proposal needs a value"),
            });
    }
}
