namespace Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>A rule a career path adjustment must pass. Check returns its problems, or none.</summary>
public interface ICareerPathAdjustmentRule
{
    List<string> Check(CareerPathAdjustment item);
}

public sealed record CareerPathAdjustmentCodeLengthRule(int MaxLength) : ICareerPathAdjustmentRule
{
    public List<string> Check(CareerPathAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAdjustmentManagerIdLengthRule(int MaxLength) : ICareerPathAdjustmentRule
{
    public List<string> Check(CareerPathAdjustment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAdjustmentCostCenterLengthRule(int MaxLength) : ICareerPathAdjustmentRule
{
    public List<string> Check(CareerPathAdjustment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathAdjustmentRequiredCodeRule(string Message) : ICareerPathAdjustmentRule
{
    public List<string> Check(CareerPathAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path adjustment.</summary>
public sealed record CareerPathAdjustmentValidator(List<ICareerPathAdjustmentRule> Rules)
{
    public List<string> Validate(CareerPathAdjustment item)
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

    public void RequireValid(CareerPathAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathAdjustment> items)
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

/// <summary>Functions of career path adjustment rules.</summary>
public static class CareerPathAdjustmentRules
{
    public static CareerPathAdjustmentValidator DefaultCareerPathAdjustmentValidator()
    {
        return new CareerPathAdjustmentValidator(
            new List<ICareerPathAdjustmentRule>
            {
                new CareerPathAdjustmentCodeLengthRule(120),
                new CareerPathAdjustmentManagerIdLengthRule(86400),
                new CareerPathAdjustmentCostCenterLengthRule(120),
                new CareerPathAdjustmentRequiredCodeRule("career path adjustment needs a value"),
            });
    }
}
