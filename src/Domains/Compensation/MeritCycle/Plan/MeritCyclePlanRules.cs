namespace Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>A rule a merit cycle plan must pass. Check returns its problems, or none.</summary>
public interface IMeritCyclePlanRule
{
    List<string> Check(MeritCyclePlan item);
}

public sealed record MeritCyclePlanCodePrefixRule(string Prefix) : IMeritCyclePlanRule
{
    public List<string> Check(MeritCyclePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCyclePlanRequiredDescriptionRule(string Message) : IMeritCyclePlanRule
{
    public List<string> Check(MeritCyclePlan item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCyclePlanRequiredCodeRule(string Message) : IMeritCyclePlanRule
{
    public List<string> Check(MeritCyclePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle plan.</summary>
public sealed record MeritCyclePlanValidator(List<IMeritCyclePlanRule> Rules)
{
    public List<string> Validate(MeritCyclePlan item)
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

    public void RequireValid(MeritCyclePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCyclePlan> items)
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

/// <summary>Functions of merit cycle plan rules.</summary>
public static class MeritCyclePlanRules
{
    public static MeritCyclePlanValidator DefaultMeritCyclePlanValidator()
    {
        return new MeritCyclePlanValidator(
            new List<IMeritCyclePlanRule>
            {
                new MeritCyclePlanCodePrefixRule("Q"),
                new MeritCyclePlanRequiredDescriptionRule("merit cycle plan needs this field"),
                new MeritCyclePlanRequiredCodeRule("merit cycle plan needs this field"),
            });
    }
}
