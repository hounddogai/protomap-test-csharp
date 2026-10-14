namespace Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>A rule a global position variance must pass. Check returns its problems, or none.</summary>
public interface IGlobalPositionVarianceRule
{
    List<string> Check(GlobalPositionVariance item);
}

public sealed record GlobalPositionVarianceCreatedAtRangeRule(int Min, int Max) : IGlobalPositionVarianceRule
{
    public List<string> Check(GlobalPositionVariance item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalPositionVarianceRequiredSalaryBandRule(string Message) : IGlobalPositionVarianceRule
{
    public List<string> Check(GlobalPositionVariance item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionVarianceTitleLengthRule(int MaxLength) : IGlobalPositionVarianceRule
{
    public List<string> Check(GlobalPositionVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPositionVarianceMaximumRangeRule(double Min, double Max) : IGlobalPositionVarianceRule
{
    public List<string> Check(GlobalPositionVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global position variance.</summary>
public sealed record GlobalPositionVarianceValidator(List<IGlobalPositionVarianceRule> Rules)
{
    public List<string> Validate(GlobalPositionVariance item)
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

    public void RequireValid(GlobalPositionVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPositionVariance> items)
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

/// <summary>Functions of global position variance rules.</summary>
public static class GlobalPositionVarianceRules
{
    public static GlobalPositionVarianceValidator DefaultGlobalPositionVarianceValidator()
    {
        return new GlobalPositionVarianceValidator(
            new List<IGlobalPositionVarianceRule>
            {
                new GlobalPositionVarianceCreatedAtRangeRule(64, 255),
                new GlobalPositionVarianceRequiredSalaryBandRule("global position variance needs a value"),
                new GlobalPositionVarianceTitleLengthRule(32),
                new GlobalPositionVarianceMaximumRangeRule(4.9, 3689.5),
            });
    }
}
