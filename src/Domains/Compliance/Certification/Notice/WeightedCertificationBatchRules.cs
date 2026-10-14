namespace Acme.Workforce.Compliance.Certification.Notice;

/// <summary>A rule a weighted certification batch must pass. Check returns its problems, or none.</summary>
public interface IWeightedCertificationBatchRule
{
    List<string> Check(WeightedCertificationBatch item);
}

public sealed record WeightedCertificationBatchEmployeeIdLengthRule(int MaxLength) : IWeightedCertificationBatchRule
{
    public List<string> Check(WeightedCertificationBatch item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCertificationBatchRequiredEmployeeIdRule(string Message) : IWeightedCertificationBatchRule
{
    public List<string> Check(WeightedCertificationBatch item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedCertificationBatchRequiredPhoneRule(string Message) : IWeightedCertificationBatchRule
{
    public List<string> Check(WeightedCertificationBatch item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted certification batch.</summary>
public sealed record WeightedCertificationBatchValidator(List<IWeightedCertificationBatchRule> Rules)
{
    public List<string> Validate(WeightedCertificationBatch item)
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

    public void RequireValid(WeightedCertificationBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedCertificationBatch> items)
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

/// <summary>Functions of weighted certification batch rules.</summary>
public static class WeightedCertificationBatchRules
{
    public static WeightedCertificationBatchValidator DefaultWeightedCertificationBatchValidator()
    {
        return new WeightedCertificationBatchValidator(
            new List<IWeightedCertificationBatchRule>
            {
                new WeightedCertificationBatchEmployeeIdLengthRule(255),
                new WeightedCertificationBatchRequiredEmployeeIdRule("weighted certification batch needs an entry"),
                new WeightedCertificationBatchRequiredPhoneRule("weighted certification batch needs a value"),
            });
    }
}
