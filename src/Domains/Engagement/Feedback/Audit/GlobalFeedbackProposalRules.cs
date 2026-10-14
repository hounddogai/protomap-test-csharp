namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global feedback proposal must pass. Check returns its problems, or none.</summary>
public interface IGlobalFeedbackProposalRule
{
    List<string> Check(GlobalFeedbackProposal item);
}

public sealed record GlobalFeedbackProposalAllowedContactEmailRule(List<string> Allowed) : IGlobalFeedbackProposalRule
{
    public List<string> Check(GlobalFeedbackProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is expired" };
        }
        return new List<string>();
    }
}

public sealed record GlobalFeedbackProposalRequiredCategoryRule(string Message) : IGlobalFeedbackProposalRule
{
    public List<string> Check(GlobalFeedbackProposal item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalFeedbackProposalRequiredContactEmailRule(string Message) : IGlobalFeedbackProposalRule
{
    public List<string> Check(GlobalFeedbackProposal item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global feedback proposal.</summary>
public sealed record GlobalFeedbackProposalValidator(List<IGlobalFeedbackProposalRule> Rules)
{
    public List<string> Validate(GlobalFeedbackProposal item)
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

    public void RequireValid(GlobalFeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalFeedbackProposal> items)
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

/// <summary>Functions of global feedback proposal rules.</summary>
public static class GlobalFeedbackProposalRules
{
    public static GlobalFeedbackProposalValidator DefaultGlobalFeedbackProposalValidator()
    {
        return new GlobalFeedbackProposalValidator(
            new List<IGlobalFeedbackProposalRule>
            {
                new GlobalFeedbackProposalAllowedContactEmailRule(
                    new List<string> { "capped", "primary", "lenient" }),
                new GlobalFeedbackProposalRequiredCategoryRule("global feedback proposal needs this field"),
                new GlobalFeedbackProposalRequiredContactEmailRule("global feedback proposal needs this field"),
            });
    }
}
