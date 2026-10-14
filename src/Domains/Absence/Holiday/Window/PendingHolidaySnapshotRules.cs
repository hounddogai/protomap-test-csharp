namespace Acme.Workforce.Absence.Holiday.Window;

/// <summary>A rule a pending holiday snapshot must pass. Check returns its problems, or none.</summary>
public interface IPendingHolidaySnapshotRule
{
    List<string> Check(PendingHolidaySnapshot item);
}

public sealed record PendingHolidaySnapshotRequiredCodeRule(string Message) : IPendingHolidaySnapshotRule
{
    public List<string> Check(PendingHolidaySnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidaySnapshotContactEmailLengthRule(int MaxLength) : IPendingHolidaySnapshotRule
{
    public List<string> Check(PendingHolidaySnapshot item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidaySnapshotCodeLengthRule(int MaxLength) : IPendingHolidaySnapshotRule
{
    public List<string> Check(PendingHolidaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending holiday snapshot.</summary>
public sealed record PendingHolidaySnapshotValidator(List<IPendingHolidaySnapshotRule> Rules)
{
    public List<string> Validate(PendingHolidaySnapshot item)
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

    public void RequireValid(PendingHolidaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingHolidaySnapshot> items)
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

/// <summary>Functions of pending holiday snapshot rules.</summary>
public static class PendingHolidaySnapshotRules
{
    public static PendingHolidaySnapshotValidator DefaultPendingHolidaySnapshotValidator()
    {
        return new PendingHolidaySnapshotValidator(
            new List<IPendingHolidaySnapshotRule>
            {
                new PendingHolidaySnapshotRequiredCodeRule("pending holiday snapshot needs a value"),
                new PendingHolidaySnapshotContactEmailLengthRule(16),
                new PendingHolidaySnapshotCodeLengthRule(64),
            });
    }
}
