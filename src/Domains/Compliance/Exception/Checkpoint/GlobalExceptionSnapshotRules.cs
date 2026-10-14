namespace Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>A rule a global exception snapshot must pass. Check returns its problems, or none.</summary>
public interface IGlobalExceptionSnapshotRule
{
    List<string> Check(GlobalExceptionSnapshot item);
}

public sealed record GlobalExceptionSnapshotJobCodeLengthRule(int MaxLength) : IGlobalExceptionSnapshotRule
{
    public List<string> Check(GlobalExceptionSnapshot item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalExceptionSnapshotRequiredJobCodeRule(string Message) : IGlobalExceptionSnapshotRule
{
    public List<string> Check(GlobalExceptionSnapshot item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalExceptionSnapshotTitleLengthRule(int MaxLength) : IGlobalExceptionSnapshotRule
{
    public List<string> Check(GlobalExceptionSnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global exception snapshot.</summary>
public sealed record GlobalExceptionSnapshotValidator(List<IGlobalExceptionSnapshotRule> Rules)
{
    public List<string> Validate(GlobalExceptionSnapshot item)
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

    public void RequireValid(GlobalExceptionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalExceptionSnapshot> items)
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

/// <summary>Functions of global exception snapshot rules.</summary>
public static class GlobalExceptionSnapshotRules
{
    public static GlobalExceptionSnapshotValidator DefaultGlobalExceptionSnapshotValidator()
    {
        return new GlobalExceptionSnapshotValidator(
            new List<IGlobalExceptionSnapshotRule>
            {
                new GlobalExceptionSnapshotJobCodeLengthRule(64),
                new GlobalExceptionSnapshotRequiredJobCodeRule("global exception snapshot needs an entry"),
                new GlobalExceptionSnapshotTitleLengthRule(3600),
            });
    }
}
