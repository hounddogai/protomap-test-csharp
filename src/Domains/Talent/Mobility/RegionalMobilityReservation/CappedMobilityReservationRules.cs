namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>A rule a capped mobility reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedMobilityReservationRule
{
    List<string> Check(CappedMobilityReservation item);
}

public sealed record CappedMobilityReservationRequiredCodeRule(string Message) : ICappedMobilityReservationRule
{
    public List<string> Check(CappedMobilityReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedMobilityReservationFteRangeRule(double Min, double Max) : ICappedMobilityReservationRule
{
    public List<string> Check(CappedMobilityReservation item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedMobilityReservationExternalRefLengthRule(int MaxLength) : ICappedMobilityReservationRule
{
    public List<string> Check(CappedMobilityReservation item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMobilityReservationLocaleLengthRule(int MaxLength) : ICappedMobilityReservationRule
{
    public List<string> Check(CappedMobilityReservation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMobilityReservationExpiresAtRangeRule(int Min, int Max) : ICappedMobilityReservationRule
{
    public List<string> Check(CappedMobilityReservation item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped mobility reservation.</summary>
public sealed record CappedMobilityReservationValidator(List<ICappedMobilityReservationRule> Rules)
{
    public List<string> Validate(CappedMobilityReservation item)
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

    public void RequireValid(CappedMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedMobilityReservation> items)
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

/// <summary>Functions of capped mobility reservation rules.</summary>
public static class CappedMobilityReservationRules
{
    public static CappedMobilityReservationValidator DefaultCappedMobilityReservationValidator()
    {
        return new CappedMobilityReservationValidator(
            new List<ICappedMobilityReservationRule>
            {
                new CappedMobilityReservationRequiredCodeRule("capped mobility reservation needs this field"),
                new CappedMobilityReservationFteRangeRule(6.0, 4771.3),
                new CappedMobilityReservationExternalRefLengthRule(80),
                new CappedMobilityReservationLocaleLengthRule(64),
                new CappedMobilityReservationExpiresAtRangeRule(16, 80),
            });
    }
}
