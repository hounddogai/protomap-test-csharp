namespace Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>A rule a refresh variance must pass. Check returns its problems, or none.</summary>
public interface IRefreshVarianceRule
{
    List<string> Check(RefreshVariance item);
}

public sealed record RefreshVarianceCountryLengthRule(int MaxLength) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshVarianceExpiresAtRangeRule(int Min, int Max) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshVarianceRequiredCodeRule(string Message) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshVarianceRatioRangeRule(double Min, double Max) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshVarianceCodeLengthRule(int MaxLength) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshVarianceLabelLengthRule(int MaxLength) : IRefreshVarianceRule
{
    public List<string> Check(RefreshVariance item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh variance.</summary>
public sealed record RefreshVarianceValidator(List<IRefreshVarianceRule> Rules)
{
    public List<string> Validate(RefreshVariance item)
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

    public void RequireValid(RefreshVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshVariance> items)
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

/// <summary>Functions of refresh variance rules.</summary>
public static class RefreshVarianceRules
{
    public static RefreshVarianceValidator DefaultRefreshVarianceValidator()
    {
        return new RefreshVarianceValidator(
            new List<IRefreshVarianceRule>
            {
                new RefreshVarianceCountryLengthRule(120),
                new RefreshVarianceExpiresAtRangeRule(120, 1000),
                new RefreshVarianceRequiredCodeRule("refresh variance needs a value"),
                new RefreshVarianceRatioRangeRule(4.3, 4561.8),
                new RefreshVarianceCodeLengthRule(32),
                new RefreshVarianceLabelLengthRule(32),
            });
    }
}
