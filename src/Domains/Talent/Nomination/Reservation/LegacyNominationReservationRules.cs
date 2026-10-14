namespace Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>A rule a legacy nomination reservation must pass. Check returns its problems, or none.</summary>
public interface ILegacyNominationReservationRule
{
    List<string> Check(LegacyNominationReservation item);
}

public sealed record LegacyNominationReservationScoreRangeRule(double Min, double Max) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyNominationReservationCodeLengthRule(int MaxLength) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyNominationReservationLabelLengthRule(int MaxLength) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyNominationReservationRequiredSummaryRule(string Message) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyNominationReservationSummaryLengthRule(int MaxLength) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyNominationReservationCodePrefixRule(string Prefix) : ILegacyNominationReservationRule
{
    public List<string> Check(LegacyNominationReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy nomination reservation.</summary>
public sealed record LegacyNominationReservationValidator(List<ILegacyNominationReservationRule> Rules)
{
    public List<string> Validate(LegacyNominationReservation item)
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

    public void RequireValid(LegacyNominationReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyNominationReservation> items)
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

/// <summary>Functions of legacy nomination reservation rules.</summary>
public static class LegacyNominationReservationRules
{
    public static LegacyNominationReservationValidator DefaultLegacyNominationReservationValidator()
    {
        return new LegacyNominationReservationValidator(
            new List<ILegacyNominationReservationRule>
            {
                new LegacyNominationReservationScoreRangeRule(3.0, 4308.4),
                new LegacyNominationReservationCodeLengthRule(1000),
                new LegacyNominationReservationLabelLengthRule(80),
                new LegacyNominationReservationRequiredSummaryRule("legacy nomination reservation needs an entry"),
                new LegacyNominationReservationSummaryLengthRule(1000),
                new LegacyNominationReservationCodePrefixRule("Q"),
            });
    }
}
