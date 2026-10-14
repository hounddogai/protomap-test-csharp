namespace Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>A rule a nomination bundle must pass. Check returns its problems, or none.</summary>
public interface INominationBundleRule
{
    List<string> Check(NominationBundle item);
}

public sealed record NominationBundleMarginRangeRule(double Min, double Max) : INominationBundleRule
{
    public List<string> Check(NominationBundle item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationBundleRateRangeRule(double Min, double Max) : INominationBundleRule
{
    public List<string> Check(NominationBundle item)
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

public sealed record NominationBundleCodePrefixRule(string Prefix) : INominationBundleRule
{
    public List<string> Check(NominationBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination bundle.</summary>
public sealed record NominationBundleValidator(List<INominationBundleRule> Rules)
{
    public List<string> Validate(NominationBundle item)
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

    public void RequireValid(NominationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationBundle> items)
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

/// <summary>Functions of nomination bundle rules.</summary>
public static class NominationBundleRules
{
    public static NominationBundleValidator DefaultNominationBundleValidator()
    {
        return new NominationBundleValidator(
            new List<INominationBundleRule>
            {
                new NominationBundleMarginRangeRule(4.8, 1942.6),
                new NominationBundleRateRangeRule(3.5, 1511.1),
                new NominationBundleCodePrefixRule("Q"),
            });
    }
}
