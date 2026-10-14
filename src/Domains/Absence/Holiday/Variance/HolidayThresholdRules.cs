namespace Acme.Workforce.Absence.Holiday.Variance;

/// <summary>A rule a holiday threshold must pass. Check returns its problems, or none.</summary>
public interface IHolidayThresholdRule
{
    List<string> Check(HolidayThreshold item);
}

public sealed record HolidayThresholdRequiredCodeRule(string Message) : IHolidayThresholdRule
{
    public List<string> Check(HolidayThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayThresholdSequenceRangeRule(int Min, int Max) : IHolidayThresholdRule
{
    public List<string> Check(HolidayThreshold item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayThresholdChecksumLengthRule(int MaxLength) : IHolidayThresholdRule
{
    public List<string> Check(HolidayThreshold item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday threshold.</summary>
public sealed record HolidayThresholdValidator(List<IHolidayThresholdRule> Rules)
{
    public List<string> Validate(HolidayThreshold item)
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

    public void RequireValid(HolidayThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayThreshold> items)
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

/// <summary>Functions of holiday threshold rules.</summary>
public static class HolidayThresholdRules
{
    public static HolidayThresholdValidator DefaultHolidayThresholdValidator()
    {
        return new HolidayThresholdValidator(
            new List<IHolidayThresholdRule>
            {
                new HolidayThresholdRequiredCodeRule("holiday threshold needs this field"),
                new HolidayThresholdSequenceRangeRule(16, 16),
                new HolidayThresholdChecksumLengthRule(16),
            });
    }
}
