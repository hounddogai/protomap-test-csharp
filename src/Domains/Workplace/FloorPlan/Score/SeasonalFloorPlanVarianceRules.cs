namespace Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>A rule a seasonal floor plan variance must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFloorPlanVarianceRule
{
    List<string> Check(SeasonalFloorPlanVariance item);
}

public sealed record SeasonalFloorPlanVarianceSalaryBandLengthRule(int MaxLength) : ISeasonalFloorPlanVarianceRule
{
    public List<string> Check(SeasonalFloorPlanVariance item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanVarianceCodeLengthRule(int MaxLength) : ISeasonalFloorPlanVarianceRule
{
    public List<string> Check(SeasonalFloorPlanVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFloorPlanVarianceDepartmentLengthRule(int MaxLength) : ISeasonalFloorPlanVarianceRule
{
    public List<string> Check(SeasonalFloorPlanVariance item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal floor plan variance.</summary>
public sealed record SeasonalFloorPlanVarianceValidator(List<ISeasonalFloorPlanVarianceRule> Rules)
{
    public List<string> Validate(SeasonalFloorPlanVariance item)
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

    public void RequireValid(SeasonalFloorPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFloorPlanVariance> items)
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

/// <summary>Functions of seasonal floor plan variance rules.</summary>
public static class SeasonalFloorPlanVarianceRules
{
    public static SeasonalFloorPlanVarianceValidator DefaultSeasonalFloorPlanVarianceValidator()
    {
        return new SeasonalFloorPlanVarianceValidator(
            new List<ISeasonalFloorPlanVarianceRule>
            {
                new SeasonalFloorPlanVarianceSalaryBandLengthRule(255),
                new SeasonalFloorPlanVarianceCodeLengthRule(32),
                new SeasonalFloorPlanVarianceDepartmentLengthRule(3600),
            });
    }
}
