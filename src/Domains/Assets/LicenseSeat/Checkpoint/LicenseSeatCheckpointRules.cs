namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>A rule a license seat checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatCheckpointRule
{
    List<string> Check(LicenseSeatCheckpoint item);
}

public sealed record LicenseSeatCheckpointStartsAtRangeRule(int Min, int Max) : ILicenseSeatCheckpointRule
{
    public List<string> Check(LicenseSeatCheckpoint item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatCheckpointTitleLengthRule(int MaxLength) : ILicenseSeatCheckpointRule
{
    public List<string> Check(LicenseSeatCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatCheckpointCodeLengthRule(int MaxLength) : ILicenseSeatCheckpointRule
{
    public List<string> Check(LicenseSeatCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat checkpoint.</summary>
public sealed record LicenseSeatCheckpointValidator(List<ILicenseSeatCheckpointRule> Rules)
{
    public List<string> Validate(LicenseSeatCheckpoint item)
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

    public void RequireValid(LicenseSeatCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatCheckpoint> items)
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

/// <summary>Functions of license seat checkpoint rules.</summary>
public static class LicenseSeatCheckpointRules
{
    public static LicenseSeatCheckpointValidator DefaultLicenseSeatCheckpointValidator()
    {
        return new LicenseSeatCheckpointValidator(
            new List<ILicenseSeatCheckpointRule>
            {
                new LicenseSeatCheckpointStartsAtRangeRule(80, 255),
                new LicenseSeatCheckpointTitleLengthRule(32),
                new LicenseSeatCheckpointCodeLengthRule(80),
            });
    }
}
