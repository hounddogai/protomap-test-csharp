namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility checkpoint must pass. Check returns its problems, or none.</summary>
public interface IMobilityCheckpointRule
{
    List<string> Check(MobilityCheckpoint item);
}

public sealed record MobilityCheckpointAllowedLabelRule(List<string> Allowed) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record MobilityCheckpointRequiredCodeRule(string Message) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityCheckpointMaximumRangeRule(double Min, double Max) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
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

public sealed record MobilityCheckpointCategoryLengthRule(int MaxLength) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityCheckpointRequiredLabelRule(string Message) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityCheckpointNameLengthRule(int MaxLength) : IMobilityCheckpointRule
{
    public List<string> Check(MobilityCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility checkpoint.</summary>
public sealed record MobilityCheckpointValidator(List<IMobilityCheckpointRule> Rules)
{
    public List<string> Validate(MobilityCheckpoint item)
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

    public void RequireValid(MobilityCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityCheckpoint> items)
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

/// <summary>Functions of mobility checkpoint rules.</summary>
public static class MobilityCheckpointRules
{
    public static MobilityCheckpointValidator DefaultMobilityCheckpointValidator()
    {
        return new MobilityCheckpointValidator(
            new List<IMobilityCheckpointRule>
            {
                new MobilityCheckpointAllowedLabelRule(new List<string>()),
                new MobilityCheckpointRequiredCodeRule("mobility checkpoint needs a value"),
                new MobilityCheckpointMaximumRangeRule(3.1, 2332.0),
                new MobilityCheckpointCategoryLengthRule(80),
                new MobilityCheckpointRequiredLabelRule("mobility checkpoint needs a value"),
                new MobilityCheckpointNameLengthRule(86400),
            });
    }
}
