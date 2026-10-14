namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional merit cycle bundle must pass. Check returns its problems, or none.</summary>
public interface IRegionalMeritCycleBundleRule
{
    List<string> Check(RegionalMeritCycleBundle item);
}

public sealed record RegionalMeritCycleBundleRequiredCodeRule(string Message) : IRegionalMeritCycleBundleRule
{
    public List<string> Check(RegionalMeritCycleBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleBundleAllowedOwnerIdRule(List<string> Allowed) : IRegionalMeritCycleBundleRule
{
    public List<string> Check(RegionalMeritCycleBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMeritCycleBundleOwnerIdLengthRule(int MaxLength) : IRegionalMeritCycleBundleRule
{
    public List<string> Check(RegionalMeritCycleBundle item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional merit cycle bundle.</summary>
public sealed record RegionalMeritCycleBundleValidator(List<IRegionalMeritCycleBundleRule> Rules)
{
    public List<string> Validate(RegionalMeritCycleBundle item)
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

    public void RequireValid(RegionalMeritCycleBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalMeritCycleBundle> items)
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

/// <summary>Functions of regional merit cycle bundle rules.</summary>
public static class RegionalMeritCycleBundleRules
{
    public static RegionalMeritCycleBundleValidator DefaultRegionalMeritCycleBundleValidator()
    {
        return new RegionalMeritCycleBundleValidator(
            new List<IRegionalMeritCycleBundleRule>
            {
                new RegionalMeritCycleBundleRequiredCodeRule("regional merit cycle bundle needs this field"),
                new RegionalMeritCycleBundleAllowedOwnerIdRule(new List<string>()),
                new RegionalMeritCycleBundleOwnerIdLengthRule(86400),
            });
    }
}
