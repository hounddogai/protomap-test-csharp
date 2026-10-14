namespace Acme.Workforce.Talent.Nomination.Group;

/// <summary>A rule a nomination plan must pass. Check returns its problems, or none.</summary>
public interface INominationPlanRule
{
    List<string> Check(NominationPlan item);
}

public sealed record NominationPlanCodePrefixRule(string Prefix) : INominationPlanRule
{
    public List<string> Check(NominationPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationPlanRateRangeRule(double Min, double Max) : INominationPlanRule
{
    public List<string> Check(NominationPlan item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationPlanRequiredLabelRule(string Message) : INominationPlanRule
{
    public List<string> Check(NominationPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationPlanRequiredTeamRule(string Message) : INominationPlanRule
{
    public List<string> Check(NominationPlan item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination plan.</summary>
public sealed record NominationPlanValidator(List<INominationPlanRule> Rules)
{
    public List<string> Validate(NominationPlan item)
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

    public void RequireValid(NominationPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationPlan> items)
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

/// <summary>Functions of nomination plan rules.</summary>
public static class NominationPlanRules
{
    public static NominationPlanValidator DefaultNominationPlanValidator()
    {
        return new NominationPlanValidator(
            new List<INominationPlanRule>
            {
                new NominationPlanCodePrefixRule("ACM"),
                new NominationPlanRateRangeRule(8.2, 246.1),
                new NominationPlanRequiredLabelRule("nomination plan needs a value"),
                new NominationPlanRequiredTeamRule("nomination plan needs an entry"),
            });
    }
}
