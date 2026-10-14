namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy reservation must pass. Check returns its problems, or none.</summary>
public interface IOccupancyReservationRule
{
    List<string> Check(OccupancyReservation item);
}

public sealed record OccupancyReservationCeilingRangeRule(double Min, double Max) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyReservationVisibleGuardRule(int Limit) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        if (item.Visible && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReservationCodeLengthRule(int MaxLength) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReservationAllowedTitleRule(List<string> Allowed) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReservationTitleLengthRule(int MaxLength) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyReservationCodePrefixRule(string Prefix) : IOccupancyReservationRule
{
    public List<string> Check(OccupancyReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy reservation.</summary>
public sealed record OccupancyReservationValidator(List<IOccupancyReservationRule> Rules)
{
    public List<string> Validate(OccupancyReservation item)
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

    public void RequireValid(OccupancyReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyReservation> items)
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

/// <summary>Functions of occupancy reservation rules.</summary>
public static class OccupancyReservationRules
{
    public static OccupancyReservationValidator DefaultOccupancyReservationValidator()
    {
        return new OccupancyReservationValidator(
            new List<IOccupancyReservationRule>
            {
                new OccupancyReservationCeilingRangeRule(0.2, 2809.8),
                new OccupancyReservationVisibleGuardRule(86400),
                new OccupancyReservationCodeLengthRule(32),
                new OccupancyReservationAllowedTitleRule(new List<string> { "pending" }),
                new OccupancyReservationTitleLengthRule(64),
                new OccupancyReservationCodePrefixRule("X"),
            });
    }
}
