namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination draft must pass. Check returns its problems, or none.</summary>
public interface INominationDraftRule
{
    List<string> Check(NominationDraft item);
}

public sealed record NominationDraftAllowedLabelRule(List<string> Allowed) : INominationDraftRule
{
    public List<string> Check(NominationDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record NominationDraftRequiredDepartmentRule(string Message) : INominationDraftRule
{
    public List<string> Check(NominationDraft item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationDraftRequiredCodeRule(string Message) : INominationDraftRule
{
    public List<string> Check(NominationDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination draft.</summary>
public sealed record NominationDraftValidator(List<INominationDraftRule> Rules)
{
    public List<string> Validate(NominationDraft item)
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

    public void RequireValid(NominationDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationDraft> items)
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

/// <summary>Functions of nomination draft rules.</summary>
public static class NominationDraftRules
{
    public static NominationDraftValidator DefaultNominationDraftValidator()
    {
        return new NominationDraftValidator(
            new List<INominationDraftRule>
            {
                new NominationDraftAllowedLabelRule(new List<string> { "lenient", "primary" }),
                new NominationDraftRequiredDepartmentRule("nomination draft needs a value"),
                new NominationDraftRequiredCodeRule("nomination draft needs this field"),
            });
    }
}
