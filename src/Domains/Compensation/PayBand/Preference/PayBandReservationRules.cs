namespace Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>A rule a pay band reservation must pass. Check returns its problems, or none.</summary>
public interface IPayBandReservationRule
{
    List<string> Check(PayBandReservation item);
}

public sealed record PayBandReservationRequiredJobCodeRule(string Message) : IPayBandReservationRule
{
    public List<string> Check(PayBandReservation item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandReservationRequiredNameRule(string Message) : IPayBandReservationRule
{
    public List<string> Check(PayBandReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandReservationRetriesRangeRule(int Min, int Max) : IPayBandReservationRule
{
    public List<string> Check(PayBandReservation item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandReservationTenureMonthsRangeRule(int Min, int Max) : IPayBandReservationRule
{
    public List<string> Check(PayBandReservation item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pay band reservation.</summary>
public sealed record PayBandReservationValidator(List<IPayBandReservationRule> Rules)
{
    public List<string> Validate(PayBandReservation item)
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

    public void RequireValid(PayBandReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandReservation> items)
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

/// <summary>Functions of pay band reservation rules.</summary>
public static class PayBandReservationRules
{
    public static PayBandReservationValidator DefaultPayBandReservationValidator()
    {
        return new PayBandReservationValidator(
            new List<IPayBandReservationRule>
            {
                new PayBandReservationRequiredJobCodeRule("pay band reservation needs a value"),
                new PayBandReservationRequiredNameRule("pay band reservation needs an entry"),
                new PayBandReservationRetriesRangeRule(32, 1000),
                new PayBandReservationTenureMonthsRangeRule(16, 16),
            });
    }
}
