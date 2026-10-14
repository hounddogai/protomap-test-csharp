namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped nomination plan must pass. Check returns its problems, or none.</summary>
public interface ICappedNominationPlanRule
{
    List<string> Check(CappedNominationPlan item);
}

public sealed record CappedNominationPlanRequiredNotesRule(string Message) : ICappedNominationPlanRule
{
    public List<string> Check(CappedNominationPlan item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationPlanNotesLengthRule(int MaxLength) : ICappedNominationPlanRule
{
    public List<string> Check(CappedNominationPlan item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationPlanAllowedNotesRule(List<string> Allowed) : ICappedNominationPlanRule
{
    public List<string> Check(CappedNominationPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record CappedNominationPlanCodePrefixRule(string Prefix) : ICappedNominationPlanRule
{
    public List<string> Check(CappedNominationPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped nomination plan.</summary>
public sealed record CappedNominationPlanValidator(List<ICappedNominationPlanRule> Rules)
{
    public List<string> Validate(CappedNominationPlan item)
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

    public void RequireValid(CappedNominationPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedNominationPlan> items)
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

/// <summary>Functions of capped nomination plan rules.</summary>
public static class CappedNominationPlanRules
{
    public static CappedNominationPlanValidator DefaultCappedNominationPlanValidator()
    {
        return new CappedNominationPlanValidator(
            new List<ICappedNominationPlanRule>
            {
                new CappedNominationPlanRequiredNotesRule("capped nomination plan needs a value"),
                new CappedNominationPlanNotesLengthRule(80),
                new CappedNominationPlanAllowedNotesRule(new List<string> { "lenient", "capped", "lenient" }),
                new CappedNominationPlanCodePrefixRule("X"),
            });
    }
}
