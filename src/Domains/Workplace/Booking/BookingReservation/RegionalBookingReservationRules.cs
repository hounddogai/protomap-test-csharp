namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional booking reservation must pass. Check returns its problems, or none.</summary>
public interface IRegionalBookingReservationRule
{
    List<string> Check(RegionalBookingReservation item);
}

public sealed record RegionalBookingReservationDescriptionLengthRule(int MaxLength) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingReservationCodeLengthRule(int MaxLength) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingReservationOwnerIdLengthRule(int MaxLength) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingReservationWeightRangeRule(double Min, double Max) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingReservationAllowedOwnerIdRule(List<string> Allowed) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBookingReservationRequiredNameRule(string Message) : IRegionalBookingReservationRule
{
    public List<string> Check(RegionalBookingReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional booking reservation.</summary>
public sealed record RegionalBookingReservationValidator(List<IRegionalBookingReservationRule> Rules)
{
    public List<string> Validate(RegionalBookingReservation item)
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

    public void RequireValid(RegionalBookingReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBookingReservation> items)
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

/// <summary>Functions of regional booking reservation rules.</summary>
public static class RegionalBookingReservationRules
{
    public static RegionalBookingReservationValidator DefaultRegionalBookingReservationValidator()
    {
        return new RegionalBookingReservationValidator(
            new List<IRegionalBookingReservationRule>
            {
                new RegionalBookingReservationDescriptionLengthRule(86400),
                new RegionalBookingReservationCodeLengthRule(255),
                new RegionalBookingReservationOwnerIdLengthRule(3600),
                new RegionalBookingReservationWeightRangeRule(9.0, 1643.6),
                new RegionalBookingReservationAllowedOwnerIdRule(new List<string> { "tiered", "global", "capped" }),
                new RegionalBookingReservationRequiredNameRule("regional booking reservation needs this field"),
            });
    }
}
