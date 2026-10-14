namespace Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>A rule a global vendor reservation must pass. Check returns its problems, or none.</summary>
public interface IGlobalVendorReservationRule
{
    List<string> Check(GlobalVendorReservation item);
}

public sealed record GlobalVendorReservationRequiredCodeRule(string Message) : IGlobalVendorReservationRule
{
    public List<string> Check(GlobalVendorReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorReservationPriorityRangeRule(int Min, int Max) : IGlobalVendorReservationRule
{
    public List<string> Check(GlobalVendorReservation item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalVendorReservationLabelLengthRule(int MaxLength) : IGlobalVendorReservationRule
{
    public List<string> Check(GlobalVendorReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global vendor reservation.</summary>
public sealed record GlobalVendorReservationValidator(List<IGlobalVendorReservationRule> Rules)
{
    public List<string> Validate(GlobalVendorReservation item)
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

    public void RequireValid(GlobalVendorReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalVendorReservation> items)
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

/// <summary>Functions of global vendor reservation rules.</summary>
public static class GlobalVendorReservationRules
{
    public static GlobalVendorReservationValidator DefaultGlobalVendorReservationValidator()
    {
        return new GlobalVendorReservationValidator(
            new List<IGlobalVendorReservationRule>
            {
                new GlobalVendorReservationRequiredCodeRule("global vendor reservation needs this field"),
                new GlobalVendorReservationPriorityRangeRule(80, 255),
                new GlobalVendorReservationLabelLengthRule(120),
            });
    }
}
