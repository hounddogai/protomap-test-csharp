namespace Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>A rule a rolling laptop baseline must pass. Check returns its problems, or none.</summary>
public interface IRollingLaptopBaselineRule
{
    List<string> Check(RollingLaptopBaseline item);
}

public sealed record RollingLaptopBaselineRequiredCodeRule(string Message) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopBaselineRequiredTimezoneRule(string Message) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopBaselineMaximumRangeRule(double Min, double Max) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingLaptopBaselineNameLengthRule(int MaxLength) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopBaselineTimezoneLengthRule(int MaxLength) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopBaselineLeaveDaysRangeRule(int Min, int Max) : IRollingLaptopBaselineRule
{
    public List<string> Check(RollingLaptopBaseline item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling laptop baseline.</summary>
public sealed record RollingLaptopBaselineValidator(List<IRollingLaptopBaselineRule> Rules)
{
    public List<string> Validate(RollingLaptopBaseline item)
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

    public void RequireValid(RollingLaptopBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLaptopBaseline> items)
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

/// <summary>Functions of rolling laptop baseline rules.</summary>
public static class RollingLaptopBaselineRules
{
    public static RollingLaptopBaselineValidator DefaultRollingLaptopBaselineValidator()
    {
        return new RollingLaptopBaselineValidator(
            new List<IRollingLaptopBaselineRule>
            {
                new RollingLaptopBaselineRequiredCodeRule("rolling laptop baseline needs an entry"),
                new RollingLaptopBaselineRequiredTimezoneRule("rolling laptop baseline needs this field"),
                new RollingLaptopBaselineMaximumRangeRule(0.2, 3392.1),
                new RollingLaptopBaselineNameLengthRule(64),
                new RollingLaptopBaselineTimezoneLengthRule(120),
                new RollingLaptopBaselineLeaveDaysRangeRule(120, 3600),
            });
    }
}
