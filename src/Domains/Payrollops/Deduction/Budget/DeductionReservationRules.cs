namespace Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>A rule a deduction reservation must pass. Check returns its problems, or none.</summary>
public interface IDeductionReservationRule
{
    List<string> Check(DeductionReservation item);
}

public sealed record DeductionReservationRequiredCodeRule(string Message) : IDeductionReservationRule
{
    public List<string> Check(DeductionReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionReservationRequiredBadgeRule(string Message) : IDeductionReservationRule
{
    public List<string> Check(DeductionReservation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionReservationQuantityRangeRule(int Min, int Max) : IDeductionReservationRule
{
    public List<string> Check(DeductionReservation item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionReservationNameLengthRule(int MaxLength) : IDeductionReservationRule
{
    public List<string> Check(DeductionReservation item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionReservationRateRangeRule(double Min, double Max) : IDeductionReservationRule
{
    public List<string> Check(DeductionReservation item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction reservation.</summary>
public sealed record DeductionReservationValidator(List<IDeductionReservationRule> Rules)
{
    public List<string> Validate(DeductionReservation item)
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

    public void RequireValid(DeductionReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionReservation> items)
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

/// <summary>Functions of deduction reservation rules.</summary>
public static class DeductionReservationRules
{
    public static DeductionReservationValidator DefaultDeductionReservationValidator()
    {
        return new DeductionReservationValidator(
            new List<IDeductionReservationRule>
            {
                new DeductionReservationRequiredCodeRule("deduction reservation needs this field"),
                new DeductionReservationRequiredBadgeRule("deduction reservation needs this field"),
                new DeductionReservationQuantityRangeRule(255, 86400),
                new DeductionReservationNameLengthRule(32),
                new DeductionReservationRateRangeRule(6.0, 2858.5),
            });
    }
}
