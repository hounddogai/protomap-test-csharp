namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global mobility rule must pass. Check returns its problems, or none.</summary>
public interface IGlobalMobilityRuleRule
{
    List<string> Check(GlobalMobilityRule item);
}

public sealed record GlobalMobilityRuleCapacityRangeRule(int Min, int Max) : IGlobalMobilityRuleRule
{
    public List<string> Check(GlobalMobilityRule item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRuleCodeLengthRule(int MaxLength) : IGlobalMobilityRuleRule
{
    public List<string> Check(GlobalMobilityRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRuleAllowedLocationRule(List<string> Allowed) : IGlobalMobilityRuleRule
{
    public List<string> Check(GlobalMobilityRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global mobility rule.</summary>
public sealed record GlobalMobilityRuleValidator(List<IGlobalMobilityRuleRule> Rules)
{
    public List<string> Validate(GlobalMobilityRule item)
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

    public void RequireValid(GlobalMobilityRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalMobilityRule> items)
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

/// <summary>Functions of global mobility rule rules.</summary>
public static class GlobalMobilityRuleRules
{
    public static GlobalMobilityRuleValidator DefaultGlobalMobilityRuleValidator()
    {
        return new GlobalMobilityRuleValidator(
            new List<IGlobalMobilityRuleRule>
            {
                new GlobalMobilityRuleCapacityRangeRule(64, 255),
                new GlobalMobilityRuleCodeLengthRule(16),
                new GlobalMobilityRuleAllowedLocationRule(new List<string> { "seasonal" }),
            });
    }
}
