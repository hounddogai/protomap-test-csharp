namespace Acme.Workforce.Workplace.Booking.Segment;

/// <summary>A rule a primary booking metric must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBookingMetricRule
{
    List<string> Check(PrimaryBookingMetric item);
}

public sealed record PrimaryBookingMetricRequiredCodeRule(string Message) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingMetricHoursRangeRule(double Min, double Max) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingMetricContactEmailLengthRule(int MaxLength) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingMetricActiveGuardRule(double Limit) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        if (item.Active && item.Ratio > Limit)
        {
            return new List<string> { $"ratio {item.Ratio} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingMetricRequiredContactEmailRule(string Message) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingMetricCodePrefixRule(string Prefix) : IPrimaryBookingMetricRule
{
    public List<string> Check(PrimaryBookingMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary booking metric.</summary>
public sealed record PrimaryBookingMetricValidator(List<IPrimaryBookingMetricRule> Rules)
{
    public List<string> Validate(PrimaryBookingMetric item)
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

    public void RequireValid(PrimaryBookingMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBookingMetric> items)
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

/// <summary>Functions of primary booking metric rules.</summary>
public static class PrimaryBookingMetricRules
{
    public static PrimaryBookingMetricValidator DefaultPrimaryBookingMetricValidator()
    {
        return new PrimaryBookingMetricValidator(
            new List<IPrimaryBookingMetricRule>
            {
                new PrimaryBookingMetricRequiredCodeRule("primary booking metric needs this field"),
                new PrimaryBookingMetricHoursRangeRule(2.0, 231.9),
                new PrimaryBookingMetricContactEmailLengthRule(86400),
                new PrimaryBookingMetricActiveGuardRule(9.6),
                new PrimaryBookingMetricRequiredContactEmailRule("primary booking metric needs this field"),
                new PrimaryBookingMetricCodePrefixRule("ACM"),
            });
    }
}
