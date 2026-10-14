namespace Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>A rule a regional adjustment reservation must pass. Check returns its problems, or none.</summary>
public interface IRegionalAdjustmentReservationRule
{
    List<string> Check(RegionalAdjustmentReservation item);
}

public sealed record RegionalAdjustmentReservationCodePrefixRule(string Prefix) : IRegionalAdjustmentReservationRule
{
    public List<string> Check(RegionalAdjustmentReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAdjustmentReservationSequenceRangeRule(int Min, int Max) : IRegionalAdjustmentReservationRule
{
    public List<string> Check(RegionalAdjustmentReservation item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAdjustmentReservationCategoryLengthRule(int MaxLength) : IRegionalAdjustmentReservationRule
{
    public List<string> Check(RegionalAdjustmentReservation item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional adjustment reservation.</summary>
public sealed record RegionalAdjustmentReservationValidator(List<IRegionalAdjustmentReservationRule> Rules)
{
    public List<string> Validate(RegionalAdjustmentReservation item)
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

    public void RequireValid(RegionalAdjustmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAdjustmentReservation> items)
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

/// <summary>Functions of regional adjustment reservation rules.</summary>
public static class RegionalAdjustmentReservationRules
{
    public static RegionalAdjustmentReservationValidator DefaultRegionalAdjustmentReservationValidator()
    {
        return new RegionalAdjustmentReservationValidator(
            new List<IRegionalAdjustmentReservationRule>
            {
                new RegionalAdjustmentReservationCodePrefixRule("REF"),
                new RegionalAdjustmentReservationSequenceRangeRule(16, 3600),
                new RegionalAdjustmentReservationCategoryLengthRule(120),
            });
    }
}
