namespace Acme.Workforce.Talent.Pool.Milestone;

/// <summary>A rule a global pool snapshot must pass. Check returns its problems, or none.</summary>
public interface IGlobalPoolSnapshotRule
{
    List<string> Check(GlobalPoolSnapshot item);
}

public sealed record GlobalPoolSnapshotRequiredTitleRule(string Message) : IGlobalPoolSnapshotRule
{
    public List<string> Check(GlobalPoolSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolSnapshotUnitLengthRule(int MaxLength) : IGlobalPoolSnapshotRule
{
    public List<string> Check(GlobalPoolSnapshot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolSnapshotRequiredCodeRule(string Message) : IGlobalPoolSnapshotRule
{
    public List<string> Check(GlobalPoolSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global pool snapshot.</summary>
public sealed record GlobalPoolSnapshotValidator(List<IGlobalPoolSnapshotRule> Rules)
{
    public List<string> Validate(GlobalPoolSnapshot item)
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

    public void RequireValid(GlobalPoolSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPoolSnapshot> items)
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

/// <summary>Functions of global pool snapshot rules.</summary>
public static class GlobalPoolSnapshotRules
{
    public static GlobalPoolSnapshotValidator DefaultGlobalPoolSnapshotValidator()
    {
        return new GlobalPoolSnapshotValidator(
            new List<IGlobalPoolSnapshotRule>
            {
                new GlobalPoolSnapshotRequiredTitleRule("global pool snapshot needs this field"),
                new GlobalPoolSnapshotUnitLengthRule(120),
                new GlobalPoolSnapshotRequiredCodeRule("global pool snapshot needs this field"),
            });
    }
}
