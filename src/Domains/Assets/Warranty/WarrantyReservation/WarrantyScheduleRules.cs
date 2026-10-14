namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>A rule a warranty schedule must pass. Check returns its problems, or none.</summary>
public interface IWarrantyScheduleRule
{
    List<string> Check(WarrantySchedule item);
}

public sealed record WarrantyScheduleCostCenterLengthRule(int MaxLength) : IWarrantyScheduleRule
{
    public List<string> Check(WarrantySchedule item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyScheduleRequiredCodeRule(string Message) : IWarrantyScheduleRule
{
    public List<string> Check(WarrantySchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyScheduleRequiredCostCenterRule(string Message) : IWarrantyScheduleRule
{
    public List<string> Check(WarrantySchedule item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty schedule.</summary>
public sealed record WarrantyScheduleValidator(List<IWarrantyScheduleRule> Rules)
{
    public List<string> Validate(WarrantySchedule item)
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

    public void RequireValid(WarrantySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantySchedule> items)
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

/// <summary>Functions of warranty schedule rules.</summary>
public static class WarrantyScheduleRules
{
    public static WarrantyScheduleValidator DefaultWarrantyScheduleValidator()
    {
        return new WarrantyScheduleValidator(
            new List<IWarrantyScheduleRule>
            {
                new WarrantyScheduleCostCenterLengthRule(32),
                new WarrantyScheduleRequiredCodeRule("warranty schedule needs an entry"),
                new WarrantyScheduleRequiredCostCenterRule("warranty schedule needs this field"),
            });
    }
}
