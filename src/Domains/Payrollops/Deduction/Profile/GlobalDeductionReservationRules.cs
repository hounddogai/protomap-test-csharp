namespace Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>A rule a global deduction reservation must pass. Check returns its problems, or none.</summary>
public interface IGlobalDeductionReservationRule
{
    List<string> Check(GlobalDeductionReservation item);
}

public sealed record GlobalDeductionReservationTitleLengthRule(int MaxLength) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionReservationRequiredTitleRule(string Message) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionReservationRatingRangeRule(double Min, double Max) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionReservationCodePrefixRule(string Prefix) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalDeductionReservationAmountRangeRule(double Min, double Max) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalDeductionReservationScoreRangeRule(double Min, double Max) : IGlobalDeductionReservationRule
{
    public List<string> Check(GlobalDeductionReservation item)
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

/// <summary>Runs every rule of a global deduction reservation.</summary>
public sealed record GlobalDeductionReservationValidator(List<IGlobalDeductionReservationRule> Rules)
{
    public List<string> Validate(GlobalDeductionReservation item)
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

    public void RequireValid(GlobalDeductionReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalDeductionReservation> items)
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

/// <summary>Functions of global deduction reservation rules.</summary>
public static class GlobalDeductionReservationRules
{
    public static GlobalDeductionReservationValidator DefaultGlobalDeductionReservationValidator()
    {
        return new GlobalDeductionReservationValidator(
            new List<IGlobalDeductionReservationRule>
            {
                new GlobalDeductionReservationTitleLengthRule(32),
                new GlobalDeductionReservationRequiredTitleRule("global deduction reservation needs an entry"),
                new GlobalDeductionReservationRatingRangeRule(3.5, 623.1),
                new GlobalDeductionReservationCodePrefixRule("Q"),
                new GlobalDeductionReservationAmountRangeRule(8.7, 1925.0),
                new GlobalDeductionReservationScoreRangeRule(6.3, 4736.9),
            });
    }
}
