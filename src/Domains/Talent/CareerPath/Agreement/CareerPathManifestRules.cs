namespace Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>A rule a career path manifest must pass. Check returns its problems, or none.</summary>
public interface ICareerPathManifestRule
{
    List<string> Check(CareerPathManifest item);
}

public sealed record CareerPathManifestRequiredPhoneRule(string Message) : ICareerPathManifestRule
{
    public List<string> Check(CareerPathManifest item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathManifestRequiredCodeRule(string Message) : ICareerPathManifestRule
{
    public List<string> Check(CareerPathManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathManifestAmountRangeRule(double Min, double Max) : ICareerPathManifestRule
{
    public List<string> Check(CareerPathManifest item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path manifest.</summary>
public sealed record CareerPathManifestValidator(List<ICareerPathManifestRule> Rules)
{
    public List<string> Validate(CareerPathManifest item)
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

    public void RequireValid(CareerPathManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathManifest> items)
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

/// <summary>Functions of career path manifest rules.</summary>
public static class CareerPathManifestRules
{
    public static CareerPathManifestValidator DefaultCareerPathManifestValidator()
    {
        return new CareerPathManifestValidator(
            new List<ICareerPathManifestRule>
            {
                new CareerPathManifestRequiredPhoneRule("career path manifest needs an entry"),
                new CareerPathManifestRequiredCodeRule("career path manifest needs a value"),
                new CareerPathManifestAmountRangeRule(8.9, 850.4),
            });
    }
}
