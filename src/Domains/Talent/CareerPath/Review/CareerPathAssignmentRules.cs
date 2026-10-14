namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path assignment must pass. Check returns its problems, or none.</summary>
public interface ICareerPathAssignmentRule
{
    List<string> Check(CareerPathAssignment item);
}

public sealed record CareerPathAssignmentRequiredNameRule(string Message) : ICareerPathAssignmentRule
{
    public List<string> Check(CareerPathAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAssignmentAllowedCategoryRule(List<string> Allowed) : ICareerPathAssignmentRule
{
    public List<string> Check(CareerPathAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAssignmentCategoryLengthRule(int MaxLength) : ICareerPathAssignmentRule
{
    public List<string> Check(CareerPathAssignment item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path assignment.</summary>
public sealed record CareerPathAssignmentValidator(List<ICareerPathAssignmentRule> Rules)
{
    public List<string> Validate(CareerPathAssignment item)
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

    public void RequireValid(CareerPathAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathAssignment> items)
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

/// <summary>Functions of career path assignment rules.</summary>
public static class CareerPathAssignmentRules
{
    public static CareerPathAssignmentValidator DefaultCareerPathAssignmentValidator()
    {
        return new CareerPathAssignmentValidator(
            new List<ICareerPathAssignmentRule>
            {
                new CareerPathAssignmentRequiredNameRule("career path assignment needs a value"),
                new CareerPathAssignmentAllowedCategoryRule(new List<string> { "seasonal" }),
                new CareerPathAssignmentCategoryLengthRule(16),
            });
    }
}
