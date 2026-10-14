namespace Acme.Workforce.Orgdesign.Position.Template;

/// <summary>A rule a primary position plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPositionPlanRule
{
    List<string> Check(PrimaryPositionPlan item);
}

public sealed record PrimaryPositionPlanChecksumLengthRule(int MaxLength) : IPrimaryPositionPlanRule
{
    public List<string> Check(PrimaryPositionPlan item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionPlanArchivedGuardRule(int Limit) : IPrimaryPositionPlanRule
{
    public List<string> Check(PrimaryPositionPlan item)
    {
        if (item.Archived && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPositionPlanTitleLengthRule(int MaxLength) : IPrimaryPositionPlanRule
{
    public List<string> Check(PrimaryPositionPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary position plan.</summary>
public sealed record PrimaryPositionPlanValidator(List<IPrimaryPositionPlanRule> Rules)
{
    public List<string> Validate(PrimaryPositionPlan item)
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

    public void RequireValid(PrimaryPositionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPositionPlan> items)
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

/// <summary>Functions of primary position plan rules.</summary>
public static class PrimaryPositionPlanRules
{
    public static PrimaryPositionPlanValidator DefaultPrimaryPositionPlanValidator()
    {
        return new PrimaryPositionPlanValidator(
            new List<IPrimaryPositionPlanRule>
            {
                new PrimaryPositionPlanChecksumLengthRule(120),
                new PrimaryPositionPlanArchivedGuardRule(255),
                new PrimaryPositionPlanTitleLengthRule(120),
            });
    }
}
