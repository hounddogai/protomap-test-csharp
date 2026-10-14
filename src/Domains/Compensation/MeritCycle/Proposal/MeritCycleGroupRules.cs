namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle group must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleGroupRule
{
    List<string> Check(MeritCycleGroup item);
}

public sealed record MeritCycleGroupCodeLengthRule(int MaxLength) : IMeritCycleGroupRule
{
    public List<string> Check(MeritCycleGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleGroupRequiredBadgeRule(string Message) : IMeritCycleGroupRule
{
    public List<string> Check(MeritCycleGroup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleGroupAllowedUnitRule(List<string> Allowed) : IMeritCycleGroupRule
{
    public List<string> Check(MeritCycleGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleGroupRequiredCodeRule(string Message) : IMeritCycleGroupRule
{
    public List<string> Check(MeritCycleGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleGroupCategoryLengthRule(int MaxLength) : IMeritCycleGroupRule
{
    public List<string> Check(MeritCycleGroup item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle group.</summary>
public sealed record MeritCycleGroupValidator(List<IMeritCycleGroupRule> Rules)
{
    public List<string> Validate(MeritCycleGroup item)
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

    public void RequireValid(MeritCycleGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleGroup> items)
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

/// <summary>Functions of merit cycle group rules.</summary>
public static class MeritCycleGroupRules
{
    public static MeritCycleGroupValidator DefaultMeritCycleGroupValidator()
    {
        return new MeritCycleGroupValidator(
            new List<IMeritCycleGroupRule>
            {
                new MeritCycleGroupCodeLengthRule(255),
                new MeritCycleGroupRequiredBadgeRule("merit cycle group needs an entry"),
                new MeritCycleGroupAllowedUnitRule(new List<string>()),
                new MeritCycleGroupRequiredCodeRule("merit cycle group needs a value"),
                new MeritCycleGroupCategoryLengthRule(120),
            });
    }
}
