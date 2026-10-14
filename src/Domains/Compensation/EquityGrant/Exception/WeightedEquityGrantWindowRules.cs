namespace Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>A rule a weighted equity grant window must pass. Check returns its problems, or none.</summary>
public interface IWeightedEquityGrantWindowRule
{
    List<string> Check(WeightedEquityGrantWindow item);
}

public sealed record WeightedEquityGrantWindowNameLengthRule(int MaxLength) : IWeightedEquityGrantWindowRule
{
    public List<string> Check(WeightedEquityGrantWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantWindowManagerIdLengthRule(int MaxLength) : IWeightedEquityGrantWindowRule
{
    public List<string> Check(WeightedEquityGrantWindow item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantWindowRequiredNameRule(string Message) : IWeightedEquityGrantWindowRule
{
    public List<string> Check(WeightedEquityGrantWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantWindowRequiredCountryRule(string Message) : IWeightedEquityGrantWindowRule
{
    public List<string> Check(WeightedEquityGrantWindow item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedEquityGrantWindowCountryLengthRule(int MaxLength) : IWeightedEquityGrantWindowRule
{
    public List<string> Check(WeightedEquityGrantWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted equity grant window.</summary>
public sealed record WeightedEquityGrantWindowValidator(List<IWeightedEquityGrantWindowRule> Rules)
{
    public List<string> Validate(WeightedEquityGrantWindow item)
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

    public void RequireValid(WeightedEquityGrantWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedEquityGrantWindow> items)
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

/// <summary>Functions of weighted equity grant window rules.</summary>
public static class WeightedEquityGrantWindowRules
{
    public static WeightedEquityGrantWindowValidator DefaultWeightedEquityGrantWindowValidator()
    {
        return new WeightedEquityGrantWindowValidator(
            new List<IWeightedEquityGrantWindowRule>
            {
                new WeightedEquityGrantWindowNameLengthRule(64),
                new WeightedEquityGrantWindowManagerIdLengthRule(80),
                new WeightedEquityGrantWindowRequiredNameRule("weighted equity grant window needs an entry"),
                new WeightedEquityGrantWindowRequiredCountryRule("weighted equity grant window needs this field"),
                new WeightedEquityGrantWindowCountryLengthRule(3600),
            });
    }
}
