namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal booking segment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBookingSegmentRule
{
    List<string> Check(SeasonalBookingSegment item);
}

public sealed record SeasonalBookingSegmentSkillLengthRule(int MaxLength) : ISeasonalBookingSegmentRule
{
    public List<string> Check(SeasonalBookingSegment item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingSegmentNameLengthRule(int MaxLength) : ISeasonalBookingSegmentRule
{
    public List<string> Check(SeasonalBookingSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingSegmentCapacityRangeRule(int Min, int Max) : ISeasonalBookingSegmentRule
{
    public List<string> Check(SeasonalBookingSegment item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingSegmentArchivedGuardRule(int Limit) : ISeasonalBookingSegmentRule
{
    public List<string> Check(SeasonalBookingSegment item)
    {
        if (item.Archived && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingSegmentAllowedNameRule(List<string> Allowed) : ISeasonalBookingSegmentRule
{
    public List<string> Check(SeasonalBookingSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal booking segment.</summary>
public sealed record SeasonalBookingSegmentValidator(List<ISeasonalBookingSegmentRule> Rules)
{
    public List<string> Validate(SeasonalBookingSegment item)
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

    public void RequireValid(SeasonalBookingSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBookingSegment> items)
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

/// <summary>Functions of seasonal booking segment rules.</summary>
public static class SeasonalBookingSegmentRules
{
    public static SeasonalBookingSegmentValidator DefaultSeasonalBookingSegmentValidator()
    {
        return new SeasonalBookingSegmentValidator(
            new List<ISeasonalBookingSegmentRule>
            {
                new SeasonalBookingSegmentSkillLengthRule(86400),
                new SeasonalBookingSegmentNameLengthRule(64),
                new SeasonalBookingSegmentCapacityRangeRule(120, 255),
                new SeasonalBookingSegmentArchivedGuardRule(86400),
                new SeasonalBookingSegmentAllowedNameRule(new List<string> { "primary" }),
            });
    }
}
