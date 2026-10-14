namespace Acme.Workforce.Talent.Mobility.Review;

/// <summary>A rule a scheduled mobility rollup must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityRollupRule
{
    List<string> Check(ScheduledMobilityRollup item);
}

public sealed record ScheduledMobilityRollupQuantityRangeRule(int Min, int Max) : IScheduledMobilityRollupRule
{
    public List<string> Check(ScheduledMobilityRollup item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledMobilityRollupManagerIdLengthRule(int MaxLength) : IScheduledMobilityRollupRule
{
    public List<string> Check(ScheduledMobilityRollup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRollupPhoneLengthRule(int MaxLength) : IScheduledMobilityRollupRule
{
    public List<string> Check(ScheduledMobilityRollup item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRollupRequiredCodeRule(string Message) : IScheduledMobilityRollupRule
{
    public List<string> Check(ScheduledMobilityRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityRollupRequiredPhoneRule(string Message) : IScheduledMobilityRollupRule
{
    public List<string> Check(ScheduledMobilityRollup item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility rollup.</summary>
public sealed record ScheduledMobilityRollupValidator(List<IScheduledMobilityRollupRule> Rules)
{
    public List<string> Validate(ScheduledMobilityRollup item)
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

    public void RequireValid(ScheduledMobilityRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityRollup> items)
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

/// <summary>Functions of scheduled mobility rollup rules.</summary>
public static class ScheduledMobilityRollupRules
{
    public static ScheduledMobilityRollupValidator DefaultScheduledMobilityRollupValidator()
    {
        return new ScheduledMobilityRollupValidator(
            new List<IScheduledMobilityRollupRule>
            {
                new ScheduledMobilityRollupQuantityRangeRule(32, 80),
                new ScheduledMobilityRollupManagerIdLengthRule(120),
                new ScheduledMobilityRollupPhoneLengthRule(80),
                new ScheduledMobilityRollupRequiredCodeRule("scheduled mobility rollup needs an entry"),
                new ScheduledMobilityRollupRequiredPhoneRule("scheduled mobility rollup needs an entry"),
            });
    }
}
