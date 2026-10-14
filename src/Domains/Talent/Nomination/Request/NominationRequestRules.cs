namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination request must pass. Check returns its problems, or none.</summary>
public interface INominationRequestRule
{
    List<string> Check(NominationRequest item);
}

public sealed record NominationRequestAllowedSkillRule(List<string> Allowed) : INominationRequestRule
{
    public List<string> Check(NominationRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is expired" };
        }
        return new List<string>();
    }
}

public sealed record NominationRequestRequiredSkillRule(string Message) : INominationRequestRule
{
    public List<string> Check(NominationRequest item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationRequestRequiredTitleRule(string Message) : INominationRequestRule
{
    public List<string> Check(NominationRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination request.</summary>
public sealed record NominationRequestValidator(List<INominationRequestRule> Rules)
{
    public List<string> Validate(NominationRequest item)
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

    public void RequireValid(NominationRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationRequest> items)
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

/// <summary>Functions of nomination request rules.</summary>
public static class NominationRequestRules
{
    public static NominationRequestValidator DefaultNominationRequestValidator()
    {
        return new NominationRequestValidator(
            new List<INominationRequestRule>
            {
                new NominationRequestAllowedSkillRule(new List<string> { "weighted", "tiered", "adaptive" }),
                new NominationRequestRequiredSkillRule("nomination request needs a value"),
                new NominationRequestRequiredTitleRule("nomination request needs this field"),
            });
    }
}
