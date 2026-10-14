namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle batch must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleBatchRule
{
    List<string> Check(MeritCycleBatch item);
}

public sealed record MeritCycleBatchExternalRefLengthRule(int MaxLength) : IMeritCycleBatchRule
{
    public List<string> Check(MeritCycleBatch item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBatchAllowedManagerIdRule(List<string> Allowed) : IMeritCycleBatchRule
{
    public List<string> Check(MeritCycleBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBatchRequiredManagerIdRule(string Message) : IMeritCycleBatchRule
{
    public List<string> Check(MeritCycleBatch item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBatchRequiredCodeRule(string Message) : IMeritCycleBatchRule
{
    public List<string> Check(MeritCycleBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleBatchCreatedAtRangeRule(int Min, int Max) : IMeritCycleBatchRule
{
    public List<string> Check(MeritCycleBatch item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle batch.</summary>
public sealed record MeritCycleBatchValidator(List<IMeritCycleBatchRule> Rules)
{
    public List<string> Validate(MeritCycleBatch item)
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

    public void RequireValid(MeritCycleBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleBatch> items)
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

/// <summary>Functions of merit cycle batch rules.</summary>
public static class MeritCycleBatchRules
{
    public static MeritCycleBatchValidator DefaultMeritCycleBatchValidator()
    {
        return new MeritCycleBatchValidator(
            new List<IMeritCycleBatchRule>
            {
                new MeritCycleBatchExternalRefLengthRule(120),
                new MeritCycleBatchAllowedManagerIdRule(new List<string> { "default", "capped", "regional" }),
                new MeritCycleBatchRequiredManagerIdRule("merit cycle batch needs an entry"),
                new MeritCycleBatchRequiredCodeRule("merit cycle batch needs this field"),
                new MeritCycleBatchCreatedAtRangeRule(32, 255),
            });
    }
}
