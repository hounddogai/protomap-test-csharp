namespace Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>A rule a career path cycle must pass. Check returns its problems, or none.</summary>
public interface ICareerPathCycleRule
{
    List<string> Check(CareerPathCycle item);
}

public sealed record CareerPathCycleCodePrefixRule(string Prefix) : ICareerPathCycleRule
{
    public List<string> Check(CareerPathCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCycleChecksumLengthRule(int MaxLength) : ICareerPathCycleRule
{
    public List<string> Check(CareerPathCycle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathCycleMaximumRangeRule(double Min, double Max) : ICareerPathCycleRule
{
    public List<string> Check(CareerPathCycle item)
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

/// <summary>Runs every rule of a career path cycle.</summary>
public sealed record CareerPathCycleValidator(List<ICareerPathCycleRule> Rules)
{
    public List<string> Validate(CareerPathCycle item)
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

    public void RequireValid(CareerPathCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathCycle> items)
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

/// <summary>Functions of career path cycle rules.</summary>
public static class CareerPathCycleRules
{
    public static CareerPathCycleValidator DefaultCareerPathCycleValidator()
    {
        return new CareerPathCycleValidator(
            new List<ICareerPathCycleRule>
            {
                new CareerPathCycleCodePrefixRule("REF"),
                new CareerPathCycleChecksumLengthRule(32),
                new CareerPathCycleMaximumRangeRule(9.9, 267.2),
            });
    }
}
