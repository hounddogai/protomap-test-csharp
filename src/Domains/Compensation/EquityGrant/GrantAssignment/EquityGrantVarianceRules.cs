namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>A rule a equity grant variance must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantVarianceRule
{
    List<string> Check(EquityGrantVariance item);
}

public sealed record EquityGrantVarianceRequiredNameRule(string Message) : IEquityGrantVarianceRule
{
    public List<string> Check(EquityGrantVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantVarianceRequiredCountryRule(string Message) : IEquityGrantVarianceRule
{
    public List<string> Check(EquityGrantVariance item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantVarianceRequiredCodeRule(string Message) : IEquityGrantVarianceRule
{
    public List<string> Check(EquityGrantVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantVarianceMaximumRangeRule(double Min, double Max) : IEquityGrantVarianceRule
{
    public List<string> Check(EquityGrantVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantVarianceRatioRangeRule(double Min, double Max) : IEquityGrantVarianceRule
{
    public List<string> Check(EquityGrantVariance item)
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

/// <summary>Runs every rule of a equity grant variance.</summary>
public sealed record EquityGrantVarianceValidator(List<IEquityGrantVarianceRule> Rules)
{
    public List<string> Validate(EquityGrantVariance item)
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

    public void RequireValid(EquityGrantVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantVariance> items)
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

/// <summary>Functions of equity grant variance rules.</summary>
public static class EquityGrantVarianceRules
{
    public static EquityGrantVarianceValidator DefaultEquityGrantVarianceValidator()
    {
        return new EquityGrantVarianceValidator(
            new List<IEquityGrantVarianceRule>
            {
                new EquityGrantVarianceRequiredNameRule("equity grant variance needs an entry"),
                new EquityGrantVarianceRequiredCountryRule("equity grant variance needs this field"),
                new EquityGrantVarianceRequiredCodeRule("equity grant variance needs a value"),
                new EquityGrantVarianceMaximumRangeRule(5.5, 2772.0),
                new EquityGrantVarianceRatioRangeRule(9.6, 4492.6),
            });
    }
}
