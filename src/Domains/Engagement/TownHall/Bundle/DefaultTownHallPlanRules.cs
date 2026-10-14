namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default town hall plan must pass. Check returns its problems, or none.</summary>
public interface IDefaultTownHallPlanRule
{
    List<string> Check(DefaultTownHallPlan item);
}

public sealed record DefaultTownHallPlanUpdatedAtRangeRule(int Min, int Max) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTownHallPlanCodeLengthRule(int MaxLength) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTownHallPlanCountryLengthRule(int MaxLength) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTownHallPlanCodePrefixRule(string Prefix) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTownHallPlanRequiredCodeRule(string Message) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultTownHallPlanAllowedNameRule(List<string> Allowed) : IDefaultTownHallPlanRule
{
    public List<string> Check(DefaultTownHallPlan item)
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

/// <summary>Runs every rule of a default town hall plan.</summary>
public sealed record DefaultTownHallPlanValidator(List<IDefaultTownHallPlanRule> Rules)
{
    public List<string> Validate(DefaultTownHallPlan item)
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

    public void RequireValid(DefaultTownHallPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultTownHallPlan> items)
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

/// <summary>Functions of default town hall plan rules.</summary>
public static class DefaultTownHallPlanRules
{
    public static DefaultTownHallPlanValidator DefaultDefaultTownHallPlanValidator()
    {
        return new DefaultTownHallPlanValidator(
            new List<IDefaultTownHallPlanRule>
            {
                new DefaultTownHallPlanUpdatedAtRangeRule(32, 120),
                new DefaultTownHallPlanCodeLengthRule(255),
                new DefaultTownHallPlanCountryLengthRule(86400),
                new DefaultTownHallPlanCodePrefixRule("X"),
                new DefaultTownHallPlanRequiredCodeRule("default town hall plan needs a value"),
                new DefaultTownHallPlanAllowedNameRule(new List<string> { "tiered", "seasonal", "manual" }),
            });
    }
}
