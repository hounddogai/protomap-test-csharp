namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>A rule a laptop digest must pass. Check returns its problems, or none.</summary>
public interface ILaptopDigestRule
{
    List<string> Check(LaptopDigest item);
}

public sealed record LaptopDigestRequiredNameRule(string Message) : ILaptopDigestRule
{
    public List<string> Check(LaptopDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopDigestRateRangeRule(double Min, double Max) : ILaptopDigestRule
{
    public List<string> Check(LaptopDigest item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopDigestUpdatedAtRangeRule(int Min, int Max) : ILaptopDigestRule
{
    public List<string> Check(LaptopDigest item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a laptop digest.</summary>
public sealed record LaptopDigestValidator(List<ILaptopDigestRule> Rules)
{
    public List<string> Validate(LaptopDigest item)
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

    public void RequireValid(LaptopDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopDigest> items)
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

/// <summary>Functions of laptop digest rules.</summary>
public static class LaptopDigestRules
{
    public static LaptopDigestValidator DefaultLaptopDigestValidator()
    {
        return new LaptopDigestValidator(
            new List<ILaptopDigestRule>
            {
                new LaptopDigestRequiredNameRule("laptop digest needs this field"),
                new LaptopDigestRateRangeRule(7.8, 4406.7),
                new LaptopDigestUpdatedAtRangeRule(16, 1000),
            });
    }
}
