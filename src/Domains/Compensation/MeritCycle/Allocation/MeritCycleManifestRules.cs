namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>A rule a merit cycle manifest must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleManifestRule
{
    List<string> Check(MeritCycleManifest item);
}

public sealed record MeritCycleManifestRequiredCodeRule(string Message) : IMeritCycleManifestRule
{
    public List<string> Check(MeritCycleManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleManifestPriorityRangeRule(int Min, int Max) : IMeritCycleManifestRule
{
    public List<string> Check(MeritCycleManifest item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleManifestNotesLengthRule(int MaxLength) : IMeritCycleManifestRule
{
    public List<string> Check(MeritCycleManifest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleManifestCodeLengthRule(int MaxLength) : IMeritCycleManifestRule
{
    public List<string> Check(MeritCycleManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleManifestUpdatedAtRangeRule(int Min, int Max) : IMeritCycleManifestRule
{
    public List<string> Check(MeritCycleManifest item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle manifest.</summary>
public sealed record MeritCycleManifestValidator(List<IMeritCycleManifestRule> Rules)
{
    public List<string> Validate(MeritCycleManifest item)
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

    public void RequireValid(MeritCycleManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleManifest> items)
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

/// <summary>Functions of merit cycle manifest rules.</summary>
public static class MeritCycleManifestRules
{
    public static MeritCycleManifestValidator DefaultMeritCycleManifestValidator()
    {
        return new MeritCycleManifestValidator(
            new List<IMeritCycleManifestRule>
            {
                new MeritCycleManifestRequiredCodeRule("merit cycle manifest needs this field"),
                new MeritCycleManifestPriorityRangeRule(80, 255),
                new MeritCycleManifestNotesLengthRule(16),
                new MeritCycleManifestCodeLengthRule(3600),
                new MeritCycleManifestUpdatedAtRangeRule(80, 80),
            });
    }
}
