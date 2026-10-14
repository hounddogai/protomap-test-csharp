namespace Acme.Workforce.Assets.Laptop.Review;

/// <summary>A rule a strict laptop reservation must pass. Check returns its problems, or none.</summary>
public interface IStrictLaptopReservationRule
{
    List<string> Check(StrictLaptopReservation item);
}

public sealed record StrictLaptopReservationCapacityRangeRule(int Min, int Max) : IStrictLaptopReservationRule
{
    public List<string> Check(StrictLaptopReservation item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopReservationArchivedGuardRule(int Limit) : IStrictLaptopReservationRule
{
    public List<string> Check(StrictLaptopReservation item)
    {
        if (item.Archived && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopReservationTitleLengthRule(int MaxLength) : IStrictLaptopReservationRule
{
    public List<string> Check(StrictLaptopReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopReservationRequiredTitleRule(string Message) : IStrictLaptopReservationRule
{
    public List<string> Check(StrictLaptopReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict laptop reservation.</summary>
public sealed record StrictLaptopReservationValidator(List<IStrictLaptopReservationRule> Rules)
{
    public List<string> Validate(StrictLaptopReservation item)
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

    public void RequireValid(StrictLaptopReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictLaptopReservation> items)
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

/// <summary>Functions of strict laptop reservation rules.</summary>
public static class StrictLaptopReservationRules
{
    public static StrictLaptopReservationValidator DefaultStrictLaptopReservationValidator()
    {
        return new StrictLaptopReservationValidator(
            new List<IStrictLaptopReservationRule>
            {
                new StrictLaptopReservationCapacityRangeRule(120, 3600),
                new StrictLaptopReservationArchivedGuardRule(16),
                new StrictLaptopReservationTitleLengthRule(16),
                new StrictLaptopReservationRequiredTitleRule("strict laptop reservation needs this field"),
            });
    }
}
