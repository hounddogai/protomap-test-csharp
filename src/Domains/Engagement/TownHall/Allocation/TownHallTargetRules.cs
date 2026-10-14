namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall target must pass. Check returns its problems, or none.</summary>
public interface ITownHallTargetRule
{
    List<string> Check(TownHallTarget item);
}

public sealed record TownHallTargetRequiredTimezoneRule(string Message) : ITownHallTargetRule
{
    public List<string> Check(TownHallTarget item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallTargetQuantityRangeRule(int Min, int Max) : ITownHallTargetRule
{
    public List<string> Check(TownHallTarget item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTargetCodeLengthRule(int MaxLength) : ITownHallTargetRule
{
    public List<string> Check(TownHallTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTargetRateRangeRule(double Min, double Max) : ITownHallTargetRule
{
    public List<string> Check(TownHallTarget item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTargetAllowedTimezoneRule(List<string> Allowed) : ITownHallTargetRule
{
    public List<string> Check(TownHallTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall target.</summary>
public sealed record TownHallTargetValidator(List<ITownHallTargetRule> Rules)
{
    public List<string> Validate(TownHallTarget item)
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

    public void RequireValid(TownHallTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallTarget> items)
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

/// <summary>Functions of town hall target rules.</summary>
public static class TownHallTargetRules
{
    public static TownHallTargetValidator DefaultTownHallTargetValidator()
    {
        return new TownHallTargetValidator(
            new List<ITownHallTargetRule>
            {
                new TownHallTargetRequiredTimezoneRule("town hall target needs this field"),
                new TownHallTargetQuantityRangeRule(80, 80),
                new TownHallTargetCodeLengthRule(1000),
                new TownHallTargetRateRangeRule(8.7, 4982.0),
                new TownHallTargetAllowedTimezoneRule(new List<string> { "fallback", "lenient" }),
            });
    }
}
