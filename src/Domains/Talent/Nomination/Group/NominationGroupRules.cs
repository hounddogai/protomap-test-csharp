namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination group must pass. Check returns its problems, or none.</summary>
public interface INominationGroupRule
{
    List<string> Check(NominationGroup item);
}

public sealed record NominationGroupAllowedContactEmailRule(List<string> Allowed) : INominationGroupRule
{
    public List<string> Check(NominationGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record NominationGroupScoreRangeRule(double Min, double Max) : INominationGroupRule
{
    public List<string> Check(NominationGroup item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationGroupRequiredLabelRule(string Message) : INominationGroupRule
{
    public List<string> Check(NominationGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationGroupRequiredCodeRule(string Message) : INominationGroupRule
{
    public List<string> Check(NominationGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination group.</summary>
public sealed record NominationGroupValidator(List<INominationGroupRule> Rules)
{
    public List<string> Validate(NominationGroup item)
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

    public void RequireValid(NominationGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationGroup> items)
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

/// <summary>Functions of nomination group rules.</summary>
public static class NominationGroupRules
{
    public static NominationGroupValidator DefaultNominationGroupValidator()
    {
        return new NominationGroupValidator(
            new List<INominationGroupRule>
            {
                new NominationGroupAllowedContactEmailRule(new List<string>()),
                new NominationGroupScoreRangeRule(5.5, 541.1),
                new NominationGroupRequiredLabelRule("nomination group needs a value"),
                new NominationGroupRequiredCodeRule("nomination group needs this field"),
            });
    }
}
