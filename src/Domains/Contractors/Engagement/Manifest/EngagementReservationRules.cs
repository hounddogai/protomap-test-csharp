namespace Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>A rule a engagement reservation must pass. Check returns its problems, or none.</summary>
public interface IEngagementReservationRule
{
    List<string> Check(EngagementReservation item);
}

public sealed record EngagementReservationRatioRangeRule(double Min, double Max) : IEngagementReservationRule
{
    public List<string> Check(EngagementReservation item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementReservationRequiredCodeRule(string Message) : IEngagementReservationRule
{
    public List<string> Check(EngagementReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementReservationLocationLengthRule(int MaxLength) : IEngagementReservationRule
{
    public List<string> Check(EngagementReservation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementReservationRequiredPhoneRule(string Message) : IEngagementReservationRule
{
    public List<string> Check(EngagementReservation item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementReservationCodePrefixRule(string Prefix) : IEngagementReservationRule
{
    public List<string> Check(EngagementReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement reservation.</summary>
public sealed record EngagementReservationValidator(List<IEngagementReservationRule> Rules)
{
    public List<string> Validate(EngagementReservation item)
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

    public void RequireValid(EngagementReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementReservation> items)
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

/// <summary>Functions of engagement reservation rules.</summary>
public static class EngagementReservationRules
{
    public static EngagementReservationValidator DefaultEngagementReservationValidator()
    {
        return new EngagementReservationValidator(
            new List<IEngagementReservationRule>
            {
                new EngagementReservationRatioRangeRule(3.8, 4810.9),
                new EngagementReservationRequiredCodeRule("engagement reservation needs this field"),
                new EngagementReservationLocationLengthRule(1000),
                new EngagementReservationRequiredPhoneRule("engagement reservation needs this field"),
                new EngagementReservationCodePrefixRule("Q"),
            });
    }
}
