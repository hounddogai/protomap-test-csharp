namespace Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>A rule a recognition snapshot must pass. Check returns its problems, or none.</summary>
public interface IRecognitionSnapshotRule
{
    List<string> Check(RecognitionSnapshot item);
}

public sealed record RecognitionSnapshotSequenceRangeRule(int Min, int Max) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionSnapshotRequiredSourceRule(string Message) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionSnapshotSalaryBandLengthRule(int MaxLength) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionSnapshotSourceLengthRule(int MaxLength) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionSnapshotRequiredCodeRule(string Message) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionSnapshotRequiredSalaryBandRule(string Message) : IRecognitionSnapshotRule
{
    public List<string> Check(RecognitionSnapshot item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition snapshot.</summary>
public sealed record RecognitionSnapshotValidator(List<IRecognitionSnapshotRule> Rules)
{
    public List<string> Validate(RecognitionSnapshot item)
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

    public void RequireValid(RecognitionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionSnapshot> items)
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

/// <summary>Functions of recognition snapshot rules.</summary>
public static class RecognitionSnapshotRules
{
    public static RecognitionSnapshotValidator DefaultRecognitionSnapshotValidator()
    {
        return new RecognitionSnapshotValidator(
            new List<IRecognitionSnapshotRule>
            {
                new RecognitionSnapshotSequenceRangeRule(3600, 3600),
                new RecognitionSnapshotRequiredSourceRule("recognition snapshot needs an entry"),
                new RecognitionSnapshotSalaryBandLengthRule(255),
                new RecognitionSnapshotSourceLengthRule(80),
                new RecognitionSnapshotRequiredCodeRule("recognition snapshot needs this field"),
                new RecognitionSnapshotRequiredSalaryBandRule("recognition snapshot needs this field"),
            });
    }
}
