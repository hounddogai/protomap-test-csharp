namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>A rule a tiered mobility reservation must pass. Check returns its problems, or none.</summary>
public interface ITieredMobilityReservationRule
{
    List<string> Check(TieredMobilityReservation item);
}

public sealed record TieredMobilityReservationMarginRangeRule(double Min, double Max) : ITieredMobilityReservationRule
{
    public List<string> Check(TieredMobilityReservation item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredMobilityReservationCodePrefixRule(string Prefix) : ITieredMobilityReservationRule
{
    public List<string> Check(TieredMobilityReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMobilityReservationOwnerIdLengthRule(int MaxLength) : ITieredMobilityReservationRule
{
    public List<string> Check(TieredMobilityReservation item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredMobilityReservationRequiredOwnerIdRule(string Message) : ITieredMobilityReservationRule
{
    public List<string> Check(TieredMobilityReservation item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered mobility reservation.</summary>
public sealed record TieredMobilityReservationValidator(List<ITieredMobilityReservationRule> Rules)
{
    public List<string> Validate(TieredMobilityReservation item)
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

    public void RequireValid(TieredMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredMobilityReservation> items)
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

/// <summary>Functions of tiered mobility reservation rules.</summary>
public static class TieredMobilityReservationRules
{
    public static TieredMobilityReservationValidator DefaultTieredMobilityReservationValidator()
    {
        return new TieredMobilityReservationValidator(
            new List<ITieredMobilityReservationRule>
            {
                new TieredMobilityReservationMarginRangeRule(3.1, 3246.2),
                new TieredMobilityReservationCodePrefixRule("X"),
                new TieredMobilityReservationOwnerIdLengthRule(120),
                new TieredMobilityReservationRequiredOwnerIdRule("tiered mobility reservation needs an entry"),
            });
    }
}
