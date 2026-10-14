namespace Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>A rule a global nomination bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalNominationBundleRule
{
    List<string> Check(GlobalNominationBundle item);
}

public sealed record GlobalNominationBundleCostRangeRule(double Min, double Max) : IGlobalNominationBundleRule
{
    public List<string> Check(GlobalNominationBundle item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationBundleNameLengthRule(int MaxLength) : IGlobalNominationBundleRule
{
    public List<string> Check(GlobalNominationBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationBundleRequiredCodeRule(string Message) : IGlobalNominationBundleRule
{
    public List<string> Check(GlobalNominationBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationBundleRequiredReviewerRule(string Message) : IGlobalNominationBundleRule
{
    public List<string> Check(GlobalNominationBundle item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationBundleReviewerLengthRule(int MaxLength) : IGlobalNominationBundleRule
{
    public List<string> Check(GlobalNominationBundle item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global nomination bundle.</summary>
public sealed record GlobalNominationBundleValidator(List<IGlobalNominationBundleRule> Rules)
{
    public List<string> Validate(GlobalNominationBundle item)
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

    public void RequireValid(GlobalNominationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalNominationBundle> items)
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

/// <summary>Functions of global nomination bundle rules.</summary>
public static class GlobalNominationBundleRules
{
    public static GlobalNominationBundleValidator DefaultGlobalNominationBundleValidator()
    {
        return new GlobalNominationBundleValidator(
            new List<IGlobalNominationBundleRule>
            {
                new GlobalNominationBundleCostRangeRule(8.4, 4847.1),
                new GlobalNominationBundleNameLengthRule(1000),
                new GlobalNominationBundleRequiredCodeRule("global nomination bundle needs a value"),
                new GlobalNominationBundleRequiredReviewerRule("global nomination bundle needs an entry"),
                new GlobalNominationBundleReviewerLengthRule(3600),
            });
    }
}
