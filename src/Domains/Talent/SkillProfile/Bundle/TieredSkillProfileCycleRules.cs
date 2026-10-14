namespace Acme.Workforce.Talent.SkillProfile.Bundle;

/// <summary>A rule a tiered skill profile cycle must pass. Check returns its problems, or none.</summary>
public interface ITieredSkillProfileCycleRule
{
    List<string> Check(TieredSkillProfileCycle item);
}

public sealed record TieredSkillProfileCycleRequiredTimezoneRule(string Message) : ITieredSkillProfileCycleRule
{
    public List<string> Check(TieredSkillProfileCycle item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileCycleRequiredSalaryBandRule(string Message) : ITieredSkillProfileCycleRule
{
    public List<string> Check(TieredSkillProfileCycle item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSkillProfileCycleRequiredCodeRule(string Message) : ITieredSkillProfileCycleRule
{
    public List<string> Check(TieredSkillProfileCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered skill profile cycle.</summary>
public sealed record TieredSkillProfileCycleValidator(List<ITieredSkillProfileCycleRule> Rules)
{
    public List<string> Validate(TieredSkillProfileCycle item)
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

    public void RequireValid(TieredSkillProfileCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSkillProfileCycle> items)
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

/// <summary>Functions of tiered skill profile cycle rules.</summary>
public static class TieredSkillProfileCycleRules
{
    public static TieredSkillProfileCycleValidator DefaultTieredSkillProfileCycleValidator()
    {
        return new TieredSkillProfileCycleValidator(
            new List<ITieredSkillProfileCycleRule>
            {
                new TieredSkillProfileCycleRequiredTimezoneRule("tiered skill profile cycle needs a value"),
                new TieredSkillProfileCycleRequiredSalaryBandRule("tiered skill profile cycle needs a value"),
                new TieredSkillProfileCycleRequiredCodeRule("tiered skill profile cycle needs a value"),
            });
    }
}
