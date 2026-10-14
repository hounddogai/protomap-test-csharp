namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict merit cycle plan must pass. Check returns its problems, or none.</summary>
public interface IStrictMeritCyclePlanRule
{
    List<string> Check(StrictMeritCyclePlan item);
}

public sealed record StrictMeritCyclePlanNameLengthRule(int MaxLength) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCyclePlanRequiredLocationRule(string Message) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCyclePlanCodeLengthRule(int MaxLength) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCyclePlanStartsAtRangeRule(int Min, int Max) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCyclePlanAllowedDescriptionRule(List<string> Allowed) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCyclePlanRequiredCurrencyRule(string Message) : IStrictMeritCyclePlanRule
{
    public List<string> Check(StrictMeritCyclePlan item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict merit cycle plan.</summary>
public sealed record StrictMeritCyclePlanValidator(List<IStrictMeritCyclePlanRule> Rules)
{
    public List<string> Validate(StrictMeritCyclePlan item)
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

    public void RequireValid(StrictMeritCyclePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictMeritCyclePlan> items)
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

/// <summary>Functions of strict merit cycle plan rules.</summary>
public static class StrictMeritCyclePlanRules
{
    public static StrictMeritCyclePlanValidator DefaultStrictMeritCyclePlanValidator()
    {
        return new StrictMeritCyclePlanValidator(
            new List<IStrictMeritCyclePlanRule>
            {
                new StrictMeritCyclePlanNameLengthRule(64),
                new StrictMeritCyclePlanRequiredLocationRule("strict merit cycle plan needs a value"),
                new StrictMeritCyclePlanCodeLengthRule(80),
                new StrictMeritCyclePlanStartsAtRangeRule(120, 3600),
                new StrictMeritCyclePlanAllowedDescriptionRule(new List<string> { "legacy", "pending", "default" }),
                new StrictMeritCyclePlanRequiredCurrencyRule("strict merit cycle plan needs an entry"),
            });
    }
}
