namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>A rule a holiday estimate must pass. Check returns its problems, or none.</summary>
public interface IHolidayEstimateRule
{
    List<string> Check(HolidayEstimate item);
}

public sealed record HolidayEstimateRequiredSegmentRule(string Message) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayEstimateRequiredSalaryBandRule(string Message) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayEstimateFloorValueRangeRule(double Min, double Max) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayEstimateCodePrefixRule(string Prefix) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayEstimateSalaryBandLengthRule(int MaxLength) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayEstimateRequiredCodeRule(string Message) : IHolidayEstimateRule
{
    public List<string> Check(HolidayEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday estimate.</summary>
public sealed record HolidayEstimateValidator(List<IHolidayEstimateRule> Rules)
{
    public List<string> Validate(HolidayEstimate item)
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

    public void RequireValid(HolidayEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayEstimate> items)
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

/// <summary>Functions of holiday estimate rules.</summary>
public static class HolidayEstimateRules
{
    public static HolidayEstimateValidator DefaultHolidayEstimateValidator()
    {
        return new HolidayEstimateValidator(
            new List<IHolidayEstimateRule>
            {
                new HolidayEstimateRequiredSegmentRule("holiday estimate needs a value"),
                new HolidayEstimateRequiredSalaryBandRule("holiday estimate needs an entry"),
                new HolidayEstimateFloorValueRangeRule(6.1, 183.0),
                new HolidayEstimateCodePrefixRule("REF"),
                new HolidayEstimateSalaryBandLengthRule(16),
                new HolidayEstimateRequiredCodeRule("holiday estimate needs this field"),
            });
    }
}
