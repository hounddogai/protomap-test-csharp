namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>A rule a rolling sentiment reservation must pass. Check returns its problems, or none.</summary>
public interface IRollingSentimentReservationRule
{
    List<string> Check(RollingSentimentReservation item);
}

public sealed record RollingSentimentReservationCodePrefixRule(string Prefix) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentReservationEmployeeIdLengthRule(int MaxLength) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentReservationCodeLengthRule(int MaxLength) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentReservationRequiredSalaryBandRule(string Message) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentReservationNameLengthRule(int MaxLength) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSentimentReservationSalaryBandLengthRule(int MaxLength) : IRollingSentimentReservationRule
{
    public List<string> Check(RollingSentimentReservation item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling sentiment reservation.</summary>
public sealed record RollingSentimentReservationValidator(List<IRollingSentimentReservationRule> Rules)
{
    public List<string> Validate(RollingSentimentReservation item)
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

    public void RequireValid(RollingSentimentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSentimentReservation> items)
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

/// <summary>Functions of rolling sentiment reservation rules.</summary>
public static class RollingSentimentReservationRules
{
    public static RollingSentimentReservationValidator DefaultRollingSentimentReservationValidator()
    {
        return new RollingSentimentReservationValidator(
            new List<IRollingSentimentReservationRule>
            {
                new RollingSentimentReservationCodePrefixRule("ACM"),
                new RollingSentimentReservationEmployeeIdLengthRule(80),
                new RollingSentimentReservationCodeLengthRule(3600),
                new RollingSentimentReservationRequiredSalaryBandRule("rolling sentiment reservation needs an entry"),
                new RollingSentimentReservationNameLengthRule(80),
                new RollingSentimentReservationSalaryBandLengthRule(1000),
            });
    }
}
