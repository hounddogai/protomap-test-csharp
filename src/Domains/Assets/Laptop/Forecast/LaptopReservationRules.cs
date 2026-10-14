namespace Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>A rule a laptop reservation must pass. Check returns its problems, or none.</summary>
public interface ILaptopReservationRule
{
    List<string> Check(LaptopReservation item);
}

public sealed record LaptopReservationLabelLengthRule(int MaxLength) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReservationSequenceRangeRule(int Min, int Max) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReservationDepartmentLengthRule(int MaxLength) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReservationCodePrefixRule(string Prefix) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReservationCeilingRangeRule(double Min, double Max) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopReservationRequiredCodeRule(string Message) : ILaptopReservationRule
{
    public List<string> Check(LaptopReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop reservation.</summary>
public sealed record LaptopReservationValidator(List<ILaptopReservationRule> Rules)
{
    public List<string> Validate(LaptopReservation item)
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

    public void RequireValid(LaptopReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopReservation> items)
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

/// <summary>Functions of laptop reservation rules.</summary>
public static class LaptopReservationRules
{
    public static LaptopReservationValidator DefaultLaptopReservationValidator()
    {
        return new LaptopReservationValidator(
            new List<ILaptopReservationRule>
            {
                new LaptopReservationLabelLengthRule(16),
                new LaptopReservationSequenceRangeRule(64, 86400),
                new LaptopReservationDepartmentLengthRule(64),
                new LaptopReservationCodePrefixRule("REF"),
                new LaptopReservationCeilingRangeRule(0.1, 2079.1),
                new LaptopReservationRequiredCodeRule("laptop reservation needs a value"),
            });
    }
}
