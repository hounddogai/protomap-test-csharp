namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>A rule a fallback training due segment must pass. Check returns its problems, or none.</summary>
public interface IFallbackTrainingDueSegmentRule
{
    List<string> Check(FallbackTrainingDueSegment item);
}

public sealed record FallbackTrainingDueSegmentRequiredLabelRule(string Message) : IFallbackTrainingDueSegmentRule
{
    public List<string> Check(FallbackTrainingDueSegment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueSegmentLabelLengthRule(int MaxLength) : IFallbackTrainingDueSegmentRule
{
    public List<string> Check(FallbackTrainingDueSegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTrainingDueSegmentMinimumRangeRule(double Min, double Max) : IFallbackTrainingDueSegmentRule
{
    public List<string> Check(FallbackTrainingDueSegment item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackTrainingDueSegmentCodeLengthRule(int MaxLength) : IFallbackTrainingDueSegmentRule
{
    public List<string> Check(FallbackTrainingDueSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback training due segment.</summary>
public sealed record FallbackTrainingDueSegmentValidator(List<IFallbackTrainingDueSegmentRule> Rules)
{
    public List<string> Validate(FallbackTrainingDueSegment item)
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

    public void RequireValid(FallbackTrainingDueSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTrainingDueSegment> items)
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

/// <summary>Functions of fallback training due segment rules.</summary>
public static class FallbackTrainingDueSegmentRules
{
    public static FallbackTrainingDueSegmentValidator DefaultFallbackTrainingDueSegmentValidator()
    {
        return new FallbackTrainingDueSegmentValidator(
            new List<IFallbackTrainingDueSegmentRule>
            {
                new FallbackTrainingDueSegmentRequiredLabelRule("fallback training due segment needs this field"),
                new FallbackTrainingDueSegmentLabelLengthRule(80),
                new FallbackTrainingDueSegmentMinimumRangeRule(9.9, 1839.2),
                new FallbackTrainingDueSegmentCodeLengthRule(32),
            });
    }
}
