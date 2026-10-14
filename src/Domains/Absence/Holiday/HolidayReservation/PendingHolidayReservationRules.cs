namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>A rule a pending holiday reservation must pass. Check returns its problems, or none.</summary>
public interface IPendingHolidayReservationRule
{
    List<string> Check(PendingHolidayReservation item);
}

public sealed record PendingHolidayReservationRequiredCodeRule(string Message) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayReservationRequiredDepartmentRule(string Message) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayReservationExpiresAtRangeRule(int Min, int Max) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayReservationDepartmentLengthRule(int MaxLength) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayReservationRateRangeRule(double Min, double Max) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingHolidayReservationCreatedAtRangeRule(int Min, int Max) : IPendingHolidayReservationRule
{
    public List<string> Check(PendingHolidayReservation item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending holiday reservation.</summary>
public sealed record PendingHolidayReservationValidator(List<IPendingHolidayReservationRule> Rules)
{
    public List<string> Validate(PendingHolidayReservation item)
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

    public void RequireValid(PendingHolidayReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingHolidayReservation> items)
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

/// <summary>Functions of pending holiday reservation rules.</summary>
public static class PendingHolidayReservationRules
{
    public static PendingHolidayReservationValidator DefaultPendingHolidayReservationValidator()
    {
        return new PendingHolidayReservationValidator(
            new List<IPendingHolidayReservationRule>
            {
                new PendingHolidayReservationRequiredCodeRule("pending holiday reservation needs this field"),
                new PendingHolidayReservationRequiredDepartmentRule("pending holiday reservation needs an entry"),
                new PendingHolidayReservationExpiresAtRangeRule(1000, 3600),
                new PendingHolidayReservationDepartmentLengthRule(80),
                new PendingHolidayReservationRateRangeRule(8.6, 963.3),
                new PendingHolidayReservationCreatedAtRangeRule(80, 86400),
            });
    }
}
