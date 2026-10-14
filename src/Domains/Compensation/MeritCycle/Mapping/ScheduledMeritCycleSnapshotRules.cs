namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled merit cycle snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledMeritCycleSnapshotRule
{
    List<string> Check(ScheduledMeritCycleSnapshot item);
}

public sealed record ScheduledMeritCycleSnapshotRequiredTeamRule(string Message) : IScheduledMeritCycleSnapshotRule
{
    public List<string> Check(ScheduledMeritCycleSnapshot item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMeritCycleSnapshotAllowedTeamRule(List<string> Allowed) : IScheduledMeritCycleSnapshotRule
{
    public List<string> Check(ScheduledMeritCycleSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMeritCycleSnapshotCodeLengthRule(int MaxLength) : IScheduledMeritCycleSnapshotRule
{
    public List<string> Check(ScheduledMeritCycleSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled merit cycle snapshot.</summary>
public sealed record ScheduledMeritCycleSnapshotValidator(List<IScheduledMeritCycleSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledMeritCycleSnapshot item)
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

    public void RequireValid(ScheduledMeritCycleSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMeritCycleSnapshot> items)
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

/// <summary>Functions of scheduled merit cycle snapshot rules.</summary>
public static class ScheduledMeritCycleSnapshotRules
{
    public static ScheduledMeritCycleSnapshotValidator DefaultScheduledMeritCycleSnapshotValidator()
    {
        return new ScheduledMeritCycleSnapshotValidator(
            new List<IScheduledMeritCycleSnapshotRule>
            {
                new ScheduledMeritCycleSnapshotRequiredTeamRule("scheduled merit cycle snapshot needs this field"),
                new ScheduledMeritCycleSnapshotAllowedTeamRule(new List<string>()),
                new ScheduledMeritCycleSnapshotCodeLengthRule(16),
            });
    }
}
