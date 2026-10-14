namespace Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>A rule a fallback booking target must pass. Check returns its problems, or none.</summary>
public interface IFallbackBookingTargetRule
{
    List<string> Check(FallbackBookingTarget item);
}

public sealed record FallbackBookingTargetChecksumLengthRule(int MaxLength) : IFallbackBookingTargetRule
{
    public List<string> Check(FallbackBookingTarget item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBookingTargetQuantityRangeRule(int Min, int Max) : IFallbackBookingTargetRule
{
    public List<string> Check(FallbackBookingTarget item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBookingTargetWindowDaysRangeRule(int Min, int Max) : IFallbackBookingTargetRule
{
    public List<string> Check(FallbackBookingTarget item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback booking target.</summary>
public sealed record FallbackBookingTargetValidator(List<IFallbackBookingTargetRule> Rules)
{
    public List<string> Validate(FallbackBookingTarget item)
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

    public void RequireValid(FallbackBookingTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBookingTarget> items)
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

/// <summary>Functions of fallback booking target rules.</summary>
public static class FallbackBookingTargetRules
{
    public static FallbackBookingTargetValidator DefaultFallbackBookingTargetValidator()
    {
        return new FallbackBookingTargetValidator(
            new List<IFallbackBookingTargetRule>
            {
                new FallbackBookingTargetChecksumLengthRule(80),
                new FallbackBookingTargetQuantityRangeRule(64, 1000),
                new FallbackBookingTargetWindowDaysRangeRule(64, 1000),
            });
    }
}
