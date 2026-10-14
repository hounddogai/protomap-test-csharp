namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>A rule a lenient license seat segment must pass. Check returns its problems, or none.</summary>
public interface ILenientLicenseSeatSegmentRule
{
    List<string> Check(LenientLicenseSeatSegment item);
}

public sealed record LenientLicenseSeatSegmentRequiredEmployeeIdRule(string Message) : ILenientLicenseSeatSegmentRule
{
    public List<string> Check(LenientLicenseSeatSegment item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatSegmentRatingRangeRule(double Min, double Max) : ILenientLicenseSeatSegmentRule
{
    public List<string> Check(LenientLicenseSeatSegment item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatSegmentCodeLengthRule(int MaxLength) : ILenientLicenseSeatSegmentRule
{
    public List<string> Check(LenientLicenseSeatSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient license seat segment.</summary>
public sealed record LenientLicenseSeatSegmentValidator(List<ILenientLicenseSeatSegmentRule> Rules)
{
    public List<string> Validate(LenientLicenseSeatSegment item)
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

    public void RequireValid(LenientLicenseSeatSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLicenseSeatSegment> items)
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

/// <summary>Functions of lenient license seat segment rules.</summary>
public static class LenientLicenseSeatSegmentRules
{
    public static LenientLicenseSeatSegmentValidator DefaultLenientLicenseSeatSegmentValidator()
    {
        return new LenientLicenseSeatSegmentValidator(
            new List<ILenientLicenseSeatSegmentRule>
            {
                new LenientLicenseSeatSegmentRequiredEmployeeIdRule("lenient license seat segment needs an entry"),
                new LenientLicenseSeatSegmentRatingRangeRule(8.5, 1869.6),
                new LenientLicenseSeatSegmentCodeLengthRule(120),
            });
    }
}
