namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered holiday snapshot must pass. Check returns its problems, or none.</summary>
public interface ITieredHolidaySnapshotRule
{
    List<string> Check(TieredHolidaySnapshot item);
}

public sealed record TieredHolidaySnapshotEmployeeIdLengthRule(int MaxLength) : ITieredHolidaySnapshotRule
{
    public List<string> Check(TieredHolidaySnapshot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidaySnapshotActiveGuardRule(int Limit) : ITieredHolidaySnapshotRule
{
    public List<string> Check(TieredHolidaySnapshot item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidaySnapshotCodeLengthRule(int MaxLength) : ITieredHolidaySnapshotRule
{
    public List<string> Check(TieredHolidaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidaySnapshotAllowedNotesRule(List<string> Allowed) : ITieredHolidaySnapshotRule
{
    public List<string> Check(TieredHolidaySnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TieredHolidaySnapshotScoreRangeRule(double Min, double Max) : ITieredHolidaySnapshotRule
{
    public List<string> Check(TieredHolidaySnapshot item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered holiday snapshot.</summary>
public sealed record TieredHolidaySnapshotValidator(List<ITieredHolidaySnapshotRule> Rules)
{
    public List<string> Validate(TieredHolidaySnapshot item)
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

    public void RequireValid(TieredHolidaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredHolidaySnapshot> items)
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

/// <summary>Functions of tiered holiday snapshot rules.</summary>
public static class TieredHolidaySnapshotRules
{
    public static TieredHolidaySnapshotValidator DefaultTieredHolidaySnapshotValidator()
    {
        return new TieredHolidaySnapshotValidator(
            new List<ITieredHolidaySnapshotRule>
            {
                new TieredHolidaySnapshotEmployeeIdLengthRule(64),
                new TieredHolidaySnapshotActiveGuardRule(3600),
                new TieredHolidaySnapshotCodeLengthRule(80),
                new TieredHolidaySnapshotAllowedNotesRule(new List<string> { "global" }),
                new TieredHolidaySnapshotScoreRangeRule(5.3, 1293.3),
            });
    }
}
