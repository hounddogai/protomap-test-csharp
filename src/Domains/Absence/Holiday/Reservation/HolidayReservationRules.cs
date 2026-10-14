namespace Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>A rule a holiday reservation must pass. Check returns its problems, or none.</summary>
public interface IHolidayReservationRule
{
    List<string> Check(HolidayReservation item);
}

public sealed record HolidayReservationRequiredTimezoneRule(string Message) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayReservationCodeLengthRule(int MaxLength) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReservationRequiredNameRule(string Message) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayReservationTimezoneLengthRule(int MaxLength) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReservationCreatedAtRangeRule(int Min, int Max) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayReservationRequiredLocaleRule(string Message) : IHolidayReservationRule
{
    public List<string> Check(HolidayReservation item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday reservation.</summary>
public sealed record HolidayReservationValidator(List<IHolidayReservationRule> Rules)
{
    public List<string> Validate(HolidayReservation item)
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

    public void RequireValid(HolidayReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayReservation> items)
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

/// <summary>Functions of holiday reservation rules.</summary>
public static class HolidayReservationRules
{
    public static HolidayReservationValidator DefaultHolidayReservationValidator()
    {
        return new HolidayReservationValidator(
            new List<IHolidayReservationRule>
            {
                new HolidayReservationRequiredTimezoneRule("holiday reservation needs a value"),
                new HolidayReservationCodeLengthRule(80),
                new HolidayReservationRequiredNameRule("holiday reservation needs an entry"),
                new HolidayReservationTimezoneLengthRule(3600),
                new HolidayReservationCreatedAtRangeRule(32, 120),
                new HolidayReservationRequiredLocaleRule("holiday reservation needs a value"),
            });
    }
}
