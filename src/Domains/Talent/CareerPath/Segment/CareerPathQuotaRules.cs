namespace Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>A rule a career path quota must pass. Check returns its problems, or none.</summary>
public interface ICareerPathQuotaRule
{
    List<string> Check(CareerPathQuota item);
}

public sealed record CareerPathQuotaRequiredReviewerRule(string Message) : ICareerPathQuotaRule
{
    public List<string> Check(CareerPathQuota item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathQuotaReviewerLengthRule(int MaxLength) : ICareerPathQuotaRule
{
    public List<string> Check(CareerPathQuota item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathQuotaRequiredCodeRule(string Message) : ICareerPathQuotaRule
{
    public List<string> Check(CareerPathQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path quota.</summary>
public sealed record CareerPathQuotaValidator(List<ICareerPathQuotaRule> Rules)
{
    public List<string> Validate(CareerPathQuota item)
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

    public void RequireValid(CareerPathQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathQuota> items)
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

/// <summary>Functions of career path quota rules.</summary>
public static class CareerPathQuotaRules
{
    public static CareerPathQuotaValidator DefaultCareerPathQuotaValidator()
    {
        return new CareerPathQuotaValidator(
            new List<ICareerPathQuotaRule>
            {
                new CareerPathQuotaRequiredReviewerRule("career path quota needs a value"),
                new CareerPathQuotaReviewerLengthRule(255),
                new CareerPathQuotaRequiredCodeRule("career path quota needs this field"),
            });
    }
}
