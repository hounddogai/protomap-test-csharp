namespace Acme.Workforce.Talent.CareerPath.Mapping;

/// <summary>A rule a career path mapping must pass. Check returns its problems, or none.</summary>
public interface ICareerPathMappingRule
{
    List<string> Check(CareerPathMapping item);
}

public sealed record CareerPathMappingRequiredCodeRule(string Message) : ICareerPathMappingRule
{
    public List<string> Check(CareerPathMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMappingRequiredLabelRule(string Message) : ICareerPathMappingRule
{
    public List<string> Check(CareerPathMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathMappingCodeLengthRule(int MaxLength) : ICareerPathMappingRule
{
    public List<string> Check(CareerPathMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path mapping.</summary>
public sealed record CareerPathMappingValidator(List<ICareerPathMappingRule> Rules)
{
    public List<string> Validate(CareerPathMapping item)
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

    public void RequireValid(CareerPathMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathMapping> items)
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

/// <summary>Functions of career path mapping rules.</summary>
public static class CareerPathMappingRules
{
    public static CareerPathMappingValidator DefaultCareerPathMappingValidator()
    {
        return new CareerPathMappingValidator(
            new List<ICareerPathMappingRule>
            {
                new CareerPathMappingRequiredCodeRule("career path mapping needs this field"),
                new CareerPathMappingRequiredLabelRule("career path mapping needs an entry"),
                new CareerPathMappingCodeLengthRule(32),
            });
    }
}
