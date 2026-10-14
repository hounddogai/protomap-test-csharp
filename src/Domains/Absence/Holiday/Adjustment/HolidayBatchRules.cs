namespace Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>A rule a holiday batch must pass. Check returns its problems, or none.</summary>
public interface IHolidayBatchRule
{
    List<string> Check(HolidayBatch item);
}

public sealed record HolidayBatchTitleLengthRule(int MaxLength) : IHolidayBatchRule
{
    public List<string> Check(HolidayBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayBatchRequiredTitleRule(string Message) : IHolidayBatchRule
{
    public List<string> Check(HolidayBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayBatchRequiredChannelRule(string Message) : IHolidayBatchRule
{
    public List<string> Check(HolidayBatch item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayBatchThresholdRangeRule(double Min, double Max) : IHolidayBatchRule
{
    public List<string> Check(HolidayBatch item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday batch.</summary>
public sealed record HolidayBatchValidator(List<IHolidayBatchRule> Rules)
{
    public List<string> Validate(HolidayBatch item)
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

    public void RequireValid(HolidayBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayBatch> items)
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

/// <summary>Functions of holiday batch rules.</summary>
public static class HolidayBatchRules
{
    public static HolidayBatchValidator DefaultHolidayBatchValidator()
    {
        return new HolidayBatchValidator(
            new List<IHolidayBatchRule>
            {
                new HolidayBatchTitleLengthRule(32),
                new HolidayBatchRequiredTitleRule("holiday batch needs this field"),
                new HolidayBatchRequiredChannelRule("holiday batch needs a value"),
                new HolidayBatchThresholdRangeRule(2.9, 599.6),
            });
    }
}
