namespace Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>A rule a rate slot must pass. Check returns its problems, or none.</summary>
public interface IRateSlotRule
{
    List<string> Check(RateSlot item);
}

public sealed record RateSlotRequiredTitleRule(string Message) : IRateSlotRule
{
    public List<string> Check(RateSlot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateSlotSkillLengthRule(int MaxLength) : IRateSlotRule
{
    public List<string> Check(RateSlot item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateSlotRequiredLocaleRule(string Message) : IRateSlotRule
{
    public List<string> Check(RateSlot item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateSlotFloorValueRangeRule(double Min, double Max) : IRateSlotRule
{
    public List<string> Check(RateSlot item)
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

public sealed record RateSlotGraceDaysRangeRule(int Min, int Max) : IRateSlotRule
{
    public List<string> Check(RateSlot item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate slot.</summary>
public sealed record RateSlotValidator(List<IRateSlotRule> Rules)
{
    public List<string> Validate(RateSlot item)
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

    public void RequireValid(RateSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateSlot> items)
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

/// <summary>Functions of rate slot rules.</summary>
public static class RateSlotRules
{
    public static RateSlotValidator DefaultRateSlotValidator()
    {
        return new RateSlotValidator(
            new List<IRateSlotRule>
            {
                new RateSlotRequiredTitleRule("rate slot needs a value"),
                new RateSlotSkillLengthRule(16),
                new RateSlotRequiredLocaleRule("rate slot needs a value"),
                new RateSlotFloorValueRangeRule(5.5, 4095.0),
                new RateSlotGraceDaysRangeRule(120, 86400),
            });
    }
}
