namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>A rule a training due segment must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueSegmentRule
{
    List<string> Check(TrainingDueSegment item);
}

public sealed record TrainingDueSegmentVisibleGuardRule(double Limit) : ITrainingDueSegmentRule
{
    public List<string> Check(TrainingDueSegment item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSegmentCountryLengthRule(int MaxLength) : ITrainingDueSegmentRule
{
    public List<string> Check(TrainingDueSegment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSegmentWindowDaysRangeRule(int Min, int Max) : ITrainingDueSegmentRule
{
    public List<string> Check(TrainingDueSegment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSegmentDiscountRangeRule(double Min, double Max) : ITrainingDueSegmentRule
{
    public List<string> Check(TrainingDueSegment item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSegmentRequiredNameRule(string Message) : ITrainingDueSegmentRule
{
    public List<string> Check(TrainingDueSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due segment.</summary>
public sealed record TrainingDueSegmentValidator(List<ITrainingDueSegmentRule> Rules)
{
    public List<string> Validate(TrainingDueSegment item)
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

    public void RequireValid(TrainingDueSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueSegment> items)
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

/// <summary>Functions of training due segment rules.</summary>
public static class TrainingDueSegmentRules
{
    public static TrainingDueSegmentValidator DefaultTrainingDueSegmentValidator()
    {
        return new TrainingDueSegmentValidator(
            new List<ITrainingDueSegmentRule>
            {
                new TrainingDueSegmentVisibleGuardRule(7.2),
                new TrainingDueSegmentCountryLengthRule(32),
                new TrainingDueSegmentWindowDaysRangeRule(32, 32),
                new TrainingDueSegmentDiscountRangeRule(9.6, 4759.5),
                new TrainingDueSegmentRequiredNameRule("training due segment needs a value"),
            });
    }
}
