namespace Acme.Workforce.Talent.Mobility.Variance;

/// <summary>A rule a mobility variance must pass. Check returns its problems, or none.</summary>
public interface IMobilityVarianceRule
{
    List<string> Check(MobilityVariance item);
}

public sealed record MobilityVarianceRateRangeRule(double Min, double Max) : IMobilityVarianceRule
{
    public List<string> Check(MobilityVariance item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityVarianceRequiredTitleRule(string Message) : IMobilityVarianceRule
{
    public List<string> Check(MobilityVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityVarianceCodeLengthRule(int MaxLength) : IMobilityVarianceRule
{
    public List<string> Check(MobilityVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility variance.</summary>
public sealed record MobilityVarianceValidator(List<IMobilityVarianceRule> Rules)
{
    public List<string> Validate(MobilityVariance item)
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

    public void RequireValid(MobilityVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityVariance> items)
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

/// <summary>Functions of mobility variance rules.</summary>
public static class MobilityVarianceRules
{
    public static MobilityVarianceValidator DefaultMobilityVarianceValidator()
    {
        return new MobilityVarianceValidator(
            new List<IMobilityVarianceRule>
            {
                new MobilityVarianceRateRangeRule(6.4, 4094.9),
                new MobilityVarianceRequiredTitleRule("mobility variance needs an entry"),
                new MobilityVarianceCodeLengthRule(16),
            });
    }
}
