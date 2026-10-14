namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path batch must pass. Check returns its problems, or none.</summary>
public interface ICareerPathBatchRule
{
    List<string> Check(CareerPathBatch item);
}

public sealed record CareerPathBatchCategoryLengthRule(int MaxLength) : ICareerPathBatchRule
{
    public List<string> Check(CareerPathBatch item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathBatchAllowedCodeRule(List<string> Allowed) : ICareerPathBatchRule
{
    public List<string> Check(CareerPathBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathBatchRequiredNameRule(string Message) : ICareerPathBatchRule
{
    public List<string> Check(CareerPathBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathBatchRequiredSummaryRule(string Message) : ICareerPathBatchRule
{
    public List<string> Check(CareerPathBatch item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path batch.</summary>
public sealed record CareerPathBatchValidator(List<ICareerPathBatchRule> Rules)
{
    public List<string> Validate(CareerPathBatch item)
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

    public void RequireValid(CareerPathBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathBatch> items)
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

/// <summary>Functions of career path batch rules.</summary>
public static class CareerPathBatchRules
{
    public static CareerPathBatchValidator DefaultCareerPathBatchValidator()
    {
        return new CareerPathBatchValidator(
            new List<ICareerPathBatchRule>
            {
                new CareerPathBatchCategoryLengthRule(32),
                new CareerPathBatchAllowedCodeRule(new List<string> { "strict" }),
                new CareerPathBatchRequiredNameRule("career path batch needs an entry"),
                new CareerPathBatchRequiredSummaryRule("career path batch needs a value"),
            });
    }
}
