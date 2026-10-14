namespace Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>A rule a capped career path digest must pass. Check returns its problems, or none.</summary>
public interface ICappedCareerPathDigestRule
{
    List<string> Check(CappedCareerPathDigest item);
}

public sealed record CappedCareerPathDigestWeightRangeRule(double Min, double Max) : ICappedCareerPathDigestRule
{
    public List<string> Check(CappedCareerPathDigest item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCareerPathDigestFteRangeRule(double Min, double Max) : ICappedCareerPathDigestRule
{
    public List<string> Check(CappedCareerPathDigest item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedCareerPathDigestBatchSizeRangeRule(int Min, int Max) : ICappedCareerPathDigestRule
{
    public List<string> Check(CappedCareerPathDigest item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped career path digest.</summary>
public sealed record CappedCareerPathDigestValidator(List<ICappedCareerPathDigestRule> Rules)
{
    public List<string> Validate(CappedCareerPathDigest item)
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

    public void RequireValid(CappedCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCareerPathDigest> items)
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

/// <summary>Functions of capped career path digest rules.</summary>
public static class CappedCareerPathDigestRules
{
    public static CappedCareerPathDigestValidator DefaultCappedCareerPathDigestValidator()
    {
        return new CappedCareerPathDigestValidator(
            new List<ICappedCareerPathDigestRule>
            {
                new CappedCareerPathDigestWeightRangeRule(2.9, 1060.5),
                new CappedCareerPathDigestFteRangeRule(9.0, 4205.0),
                new CappedCareerPathDigestBatchSizeRangeRule(32, 86400),
            });
    }
}
