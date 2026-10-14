namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>A rule a weighted town hall exception must pass. Check returns its problems, or none.</summary>
public interface IWeightedTownHallExceptionRule
{
    List<string> Check(WeightedTownHallException item);
}

public sealed record WeightedTownHallExceptionScoreRangeRule(double Min, double Max) : IWeightedTownHallExceptionRule
{
    public List<string> Check(WeightedTownHallException item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTownHallExceptionLabelLengthRule(int MaxLength) : IWeightedTownHallExceptionRule
{
    public List<string> Check(WeightedTownHallException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTownHallExceptionCapacityRangeRule(int Min, int Max) : IWeightedTownHallExceptionRule
{
    public List<string> Check(WeightedTownHallException item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTownHallExceptionRequiredNameRule(string Message) : IWeightedTownHallExceptionRule
{
    public List<string> Check(WeightedTownHallException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted town hall exception.</summary>
public sealed record WeightedTownHallExceptionValidator(List<IWeightedTownHallExceptionRule> Rules)
{
    public List<string> Validate(WeightedTownHallException item)
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

    public void RequireValid(WeightedTownHallException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTownHallException> items)
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

/// <summary>Functions of weighted town hall exception rules.</summary>
public static class WeightedTownHallExceptionRules
{
    public static WeightedTownHallExceptionValidator DefaultWeightedTownHallExceptionValidator()
    {
        return new WeightedTownHallExceptionValidator(
            new List<IWeightedTownHallExceptionRule>
            {
                new WeightedTownHallExceptionScoreRangeRule(6.4, 2657.8),
                new WeightedTownHallExceptionLabelLengthRule(1000),
                new WeightedTownHallExceptionCapacityRangeRule(1000, 86400),
                new WeightedTownHallExceptionRequiredNameRule("weighted town hall exception needs this field"),
            });
    }
}
