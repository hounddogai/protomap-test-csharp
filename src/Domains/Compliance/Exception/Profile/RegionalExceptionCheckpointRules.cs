namespace Acme.Workforce.Compliance.Exception.Profile;

/// <summary>A rule a regional exception checkpoint must pass. Check returns its problems, or none.</summary>
public interface IRegionalExceptionCheckpointRule
{
    List<string> Check(RegionalExceptionCheckpoint item);
}

public sealed record RegionalExceptionCheckpointCodeLengthRule(int MaxLength) : IRegionalExceptionCheckpointRule
{
    public List<string> Check(RegionalExceptionCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionCheckpointRequiredTeamRule(string Message) : IRegionalExceptionCheckpointRule
{
    public List<string> Check(RegionalExceptionCheckpoint item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionCheckpointTitleLengthRule(int MaxLength) : IRegionalExceptionCheckpointRule
{
    public List<string> Check(RegionalExceptionCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional exception checkpoint.</summary>
public sealed record RegionalExceptionCheckpointValidator(List<IRegionalExceptionCheckpointRule> Rules)
{
    public List<string> Validate(RegionalExceptionCheckpoint item)
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

    public void RequireValid(RegionalExceptionCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalExceptionCheckpoint> items)
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

/// <summary>Functions of regional exception checkpoint rules.</summary>
public static class RegionalExceptionCheckpointRules
{
    public static RegionalExceptionCheckpointValidator DefaultRegionalExceptionCheckpointValidator()
    {
        return new RegionalExceptionCheckpointValidator(
            new List<IRegionalExceptionCheckpointRule>
            {
                new RegionalExceptionCheckpointCodeLengthRule(1000),
                new RegionalExceptionCheckpointRequiredTeamRule("regional exception checkpoint needs a value"),
                new RegionalExceptionCheckpointTitleLengthRule(3600),
            });
    }
}
