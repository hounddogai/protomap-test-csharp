namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional merit cycle allocation must pass. Check returns its problems, or none.</summary>
public interface IRegionalMeritCycleAllocationRule
{
    List<string> Check(RegionalMeritCycleAllocation item);
}

public sealed record RegionalMeritCycleAllocationRequiredCodeRule(string Message) : IRegionalMeritCycleAllocationRule
{
    public List<string> Check(RegionalMeritCycleAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleAllocationRevisionRangeRule(int Min, int Max) : IRegionalMeritCycleAllocationRule
{
    public List<string> Check(RegionalMeritCycleAllocation item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalMeritCycleAllocationCodePrefixRule(string Prefix) : IRegionalMeritCycleAllocationRule
{
    public List<string> Check(RegionalMeritCycleAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleAllocationCodeLengthRule(int MaxLength) : IRegionalMeritCycleAllocationRule
{
    public List<string> Check(RegionalMeritCycleAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleAllocationAllowedCodeRule(List<string> Allowed) : IRegionalMeritCycleAllocationRule
{
    public List<string> Check(RegionalMeritCycleAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional merit cycle allocation.</summary>
public sealed record RegionalMeritCycleAllocationValidator(List<IRegionalMeritCycleAllocationRule> Rules)
{
    public List<string> Validate(RegionalMeritCycleAllocation item)
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

    public void RequireValid(RegionalMeritCycleAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalMeritCycleAllocation> items)
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

/// <summary>Functions of regional merit cycle allocation rules.</summary>
public static class RegionalMeritCycleAllocationRules
{
    public static RegionalMeritCycleAllocationValidator DefaultRegionalMeritCycleAllocationValidator()
    {
        return new RegionalMeritCycleAllocationValidator(
            new List<IRegionalMeritCycleAllocationRule>
            {
                new RegionalMeritCycleAllocationRequiredCodeRule("regional merit cycle allocation needs this field"),
                new RegionalMeritCycleAllocationRevisionRangeRule(16, 32),
                new RegionalMeritCycleAllocationCodePrefixRule("X"),
                new RegionalMeritCycleAllocationCodeLengthRule(120),
                new RegionalMeritCycleAllocationAllowedCodeRule(new List<string> { "tiered" }),
            });
    }
}
