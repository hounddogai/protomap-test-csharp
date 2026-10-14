namespace Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>A rule a career path snapshot must pass. Check returns its problems, or none.</summary>
public interface ICareerPathSnapshotRule
{
    List<string> Check(CareerPathSnapshot item);
}

public sealed record CareerPathSnapshotLabelLengthRule(int MaxLength) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSnapshotRequiredChannelRule(string Message) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSnapshotWindowDaysRangeRule(int Min, int Max) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathSnapshotCodePrefixRule(string Prefix) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSnapshotCapacityRangeRule(int Min, int Max) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSnapshotChannelLengthRule(int MaxLength) : ICareerPathSnapshotRule
{
    public List<string> Check(CareerPathSnapshot item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path snapshot.</summary>
public sealed record CareerPathSnapshotValidator(List<ICareerPathSnapshotRule> Rules)
{
    public List<string> Validate(CareerPathSnapshot item)
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

    public void RequireValid(CareerPathSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathSnapshot> items)
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

/// <summary>Functions of career path snapshot rules.</summary>
public static class CareerPathSnapshotRules
{
    public static CareerPathSnapshotValidator DefaultCareerPathSnapshotValidator()
    {
        return new CareerPathSnapshotValidator(
            new List<ICareerPathSnapshotRule>
            {
                new CareerPathSnapshotLabelLengthRule(32),
                new CareerPathSnapshotRequiredChannelRule("career path snapshot needs an entry"),
                new CareerPathSnapshotWindowDaysRangeRule(255, 3600),
                new CareerPathSnapshotCodePrefixRule("ACM"),
                new CareerPathSnapshotCapacityRangeRule(64, 1000),
                new CareerPathSnapshotChannelLengthRule(1000),
            });
    }
}
