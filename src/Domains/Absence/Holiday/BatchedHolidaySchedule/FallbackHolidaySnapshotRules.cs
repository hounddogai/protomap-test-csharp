namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>A rule a fallback holiday snapshot must pass. Check returns its problems, or none.</summary>
public interface IFallbackHolidaySnapshotRule
{
    List<string> Check(FallbackHolidaySnapshot item);
}

public sealed record FallbackHolidaySnapshotRequiredExternalRefRule(string Message) : IFallbackHolidaySnapshotRule
{
    public List<string> Check(FallbackHolidaySnapshot item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidaySnapshotCodeLengthRule(int MaxLength) : IFallbackHolidaySnapshotRule
{
    public List<string> Check(FallbackHolidaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackHolidaySnapshotLabelLengthRule(int MaxLength) : IFallbackHolidaySnapshotRule
{
    public List<string> Check(FallbackHolidaySnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback holiday snapshot.</summary>
public sealed record FallbackHolidaySnapshotValidator(List<IFallbackHolidaySnapshotRule> Rules)
{
    public List<string> Validate(FallbackHolidaySnapshot item)
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

    public void RequireValid(FallbackHolidaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackHolidaySnapshot> items)
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

/// <summary>Functions of fallback holiday snapshot rules.</summary>
public static class FallbackHolidaySnapshotRules
{
    public static FallbackHolidaySnapshotValidator DefaultFallbackHolidaySnapshotValidator()
    {
        return new FallbackHolidaySnapshotValidator(
            new List<IFallbackHolidaySnapshotRule>
            {
                new FallbackHolidaySnapshotRequiredExternalRefRule("fallback holiday snapshot needs an entry"),
                new FallbackHolidaySnapshotCodeLengthRule(3600),
                new FallbackHolidaySnapshotLabelLengthRule(86400),
            });
    }
}
