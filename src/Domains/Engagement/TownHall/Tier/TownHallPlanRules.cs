namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall plan must pass. Check returns its problems, or none.</summary>
public interface ITownHallPlanRule
{
    List<string> Check(TownHallPlan item);
}

public sealed record TownHallPlanRequiredExternalRefRule(string Message) : ITownHallPlanRule
{
    public List<string> Check(TownHallPlan item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallPlanNameLengthRule(int MaxLength) : ITownHallPlanRule
{
    public List<string> Check(TownHallPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPlanCodePrefixRule(string Prefix) : ITownHallPlanRule
{
    public List<string> Check(TownHallPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPlanAllowedNameRule(List<string> Allowed) : ITownHallPlanRule
{
    public List<string> Check(TownHallPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record TownHallPlanExternalRefLengthRule(int MaxLength) : ITownHallPlanRule
{
    public List<string> Check(TownHallPlan item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall plan.</summary>
public sealed record TownHallPlanValidator(List<ITownHallPlanRule> Rules)
{
    public List<string> Validate(TownHallPlan item)
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

    public void RequireValid(TownHallPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallPlan> items)
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

/// <summary>Functions of town hall plan rules.</summary>
public static class TownHallPlanRules
{
    public static TownHallPlanValidator DefaultTownHallPlanValidator()
    {
        return new TownHallPlanValidator(
            new List<ITownHallPlanRule>
            {
                new TownHallPlanRequiredExternalRefRule("town hall plan needs a value"),
                new TownHallPlanNameLengthRule(86400),
                new TownHallPlanCodePrefixRule("OPS"),
                new TownHallPlanAllowedNameRule(new List<string> { "adaptive" }),
                new TownHallPlanExternalRefLengthRule(120),
            });
    }
}
