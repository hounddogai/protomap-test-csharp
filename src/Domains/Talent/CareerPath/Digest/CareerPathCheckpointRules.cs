namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>A rule a career path checkpoint must pass. Check returns its problems, or none.</summary>
public interface ICareerPathCheckpointRule
{
    List<string> Check(CareerPathCheckpoint item);
}

public sealed record CareerPathCheckpointLabelLengthRule(int MaxLength) : ICareerPathCheckpointRule
{
    public List<string> Check(CareerPathCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCheckpointOwnerIdLengthRule(int MaxLength) : ICareerPathCheckpointRule
{
    public List<string> Check(CareerPathCheckpoint item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCheckpointRatioRangeRule(double Min, double Max) : ICareerPathCheckpointRule
{
    public List<string> Check(CareerPathCheckpoint item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCheckpointCodeLengthRule(int MaxLength) : ICareerPathCheckpointRule
{
    public List<string> Check(CareerPathCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCheckpointCeilingRangeRule(double Min, double Max) : ICareerPathCheckpointRule
{
    public List<string> Check(CareerPathCheckpoint item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a career path checkpoint.</summary>
public sealed record CareerPathCheckpointValidator(List<ICareerPathCheckpointRule> Rules)
{
    public List<string> Validate(CareerPathCheckpoint item)
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

    public void RequireValid(CareerPathCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathCheckpoint> items)
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

/// <summary>Functions of career path checkpoint rules.</summary>
public static class CareerPathCheckpointRules
{
    public static CareerPathCheckpointValidator DefaultCareerPathCheckpointValidator()
    {
        return new CareerPathCheckpointValidator(
            new List<ICareerPathCheckpointRule>
            {
                new CareerPathCheckpointLabelLengthRule(64),
                new CareerPathCheckpointOwnerIdLengthRule(80),
                new CareerPathCheckpointRatioRangeRule(8.7, 2938.7),
                new CareerPathCheckpointCodeLengthRule(64),
                new CareerPathCheckpointCeilingRangeRule(10.0, 1030.2),
            });
    }
}
