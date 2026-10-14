namespace Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>A rule a legacy booking variance must pass. Check returns its problems, or none.</summary>
public interface ILegacyBookingVarianceRule
{
    List<string> Check(LegacyBookingVariance item);
}

public sealed record LegacyBookingVarianceRequiredNameRule(string Message) : ILegacyBookingVarianceRule
{
    public List<string> Check(LegacyBookingVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingVarianceRequiredCodeRule(string Message) : ILegacyBookingVarianceRule
{
    public List<string> Check(LegacyBookingVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBookingVarianceCodeLengthRule(int MaxLength) : ILegacyBookingVarianceRule
{
    public List<string> Check(LegacyBookingVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy booking variance.</summary>
public sealed record LegacyBookingVarianceValidator(List<ILegacyBookingVarianceRule> Rules)
{
    public List<string> Validate(LegacyBookingVariance item)
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

    public void RequireValid(LegacyBookingVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBookingVariance> items)
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

/// <summary>Functions of legacy booking variance rules.</summary>
public static class LegacyBookingVarianceRules
{
    public static LegacyBookingVarianceValidator DefaultLegacyBookingVarianceValidator()
    {
        return new LegacyBookingVarianceValidator(
            new List<ILegacyBookingVarianceRule>
            {
                new LegacyBookingVarianceRequiredNameRule("legacy booking variance needs an entry"),
                new LegacyBookingVarianceRequiredCodeRule("legacy booking variance needs this field"),
                new LegacyBookingVarianceCodeLengthRule(255),
            });
    }
}
