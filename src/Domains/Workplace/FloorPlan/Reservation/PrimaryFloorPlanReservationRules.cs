namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>A rule a primary floor plan reservation must pass. Check returns its problems, or none.</summary>
public interface IPrimaryFloorPlanReservationRule
{
    List<string> Check(PrimaryFloorPlanReservation item);
}

public sealed record PrimaryFloorPlanReservationLocaleLengthRule(int MaxLength) : IPrimaryFloorPlanReservationRule
{
    public List<string> Check(PrimaryFloorPlanReservation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanReservationRequiredLocaleRule(string Message) : IPrimaryFloorPlanReservationRule
{
    public List<string> Check(PrimaryFloorPlanReservation item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanReservationNameLengthRule(int MaxLength) : IPrimaryFloorPlanReservationRule
{
    public List<string> Check(PrimaryFloorPlanReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanReservationRequiredPhoneRule(string Message) : IPrimaryFloorPlanReservationRule
{
    public List<string> Check(PrimaryFloorPlanReservation item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanReservationRequiredNameRule(string Message) : IPrimaryFloorPlanReservationRule
{
    public List<string> Check(PrimaryFloorPlanReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary floor plan reservation.</summary>
public sealed record PrimaryFloorPlanReservationValidator(List<IPrimaryFloorPlanReservationRule> Rules)
{
    public List<string> Validate(PrimaryFloorPlanReservation item)
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

    public void RequireValid(PrimaryFloorPlanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryFloorPlanReservation> items)
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

/// <summary>Functions of primary floor plan reservation rules.</summary>
public static class PrimaryFloorPlanReservationRules
{
    public static PrimaryFloorPlanReservationValidator DefaultPrimaryFloorPlanReservationValidator()
    {
        return new PrimaryFloorPlanReservationValidator(
            new List<IPrimaryFloorPlanReservationRule>
            {
                new PrimaryFloorPlanReservationLocaleLengthRule(80),
                new PrimaryFloorPlanReservationRequiredLocaleRule("primary floor plan reservation needs a value"),
                new PrimaryFloorPlanReservationNameLengthRule(1000),
                new PrimaryFloorPlanReservationRequiredPhoneRule("primary floor plan reservation needs a value"),
                new PrimaryFloorPlanReservationRequiredNameRule("primary floor plan reservation needs this field"),
            });
    }
}
