namespace Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>A rule a lenient retro pay rollup must pass. Check returns its problems, or none.</summary>
public interface ILenientRetroPayRollupRule
{
    List<string> Check(LenientRetroPayRollup item);
}

public sealed record LenientRetroPayRollupCodeLengthRule(int MaxLength) : ILenientRetroPayRollupRule
{
    public List<string> Check(LenientRetroPayRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayRollupRequiredRegionRule(string Message) : ILenientRetroPayRollupRule
{
    public List<string> Check(LenientRetroPayRollup item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRetroPayRollupFteRangeRule(double Min, double Max) : ILenientRetroPayRollupRule
{
    public List<string> Check(LenientRetroPayRollup item)
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

/// <summary>Runs every rule of a lenient retro pay rollup.</summary>
public sealed record LenientRetroPayRollupValidator(List<ILenientRetroPayRollupRule> Rules)
{
    public List<string> Validate(LenientRetroPayRollup item)
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

    public void RequireValid(LenientRetroPayRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRetroPayRollup> items)
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

/// <summary>Functions of lenient retro pay rollup rules.</summary>
public static class LenientRetroPayRollupRules
{
    public static LenientRetroPayRollupValidator DefaultLenientRetroPayRollupValidator()
    {
        return new LenientRetroPayRollupValidator(
            new List<ILenientRetroPayRollupRule>
            {
                new LenientRetroPayRollupCodeLengthRule(16),
                new LenientRetroPayRollupRequiredRegionRule("lenient retro pay rollup needs an entry"),
                new LenientRetroPayRollupFteRangeRule(3.8, 873.3),
            });
    }
}
