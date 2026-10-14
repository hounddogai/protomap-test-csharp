namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>A rule a rolling tax form adjustment must pass. Check returns its problems, or none.</summary>
public interface IRollingTaxFormAdjustmentRule
{
    List<string> Check(RollingTaxFormAdjustment item);
}

public sealed record RollingTaxFormAdjustmentRequiredCodeRule(string Message) : IRollingTaxFormAdjustmentRule
{
    public List<string> Check(RollingTaxFormAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingTaxFormAdjustmentNameLengthRule(int MaxLength) : IRollingTaxFormAdjustmentRule
{
    public List<string> Check(RollingTaxFormAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTaxFormAdjustmentPinnedGuardRule(int Limit) : IRollingTaxFormAdjustmentRule
{
    public List<string> Check(RollingTaxFormAdjustment item)
    {
        if (item.Pinned && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RollingTaxFormAdjustmentCapacityRangeRule(int Min, int Max) : IRollingTaxFormAdjustmentRule
{
    public List<string> Check(RollingTaxFormAdjustment item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTaxFormAdjustmentGradeLengthRule(int MaxLength) : IRollingTaxFormAdjustmentRule
{
    public List<string> Check(RollingTaxFormAdjustment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling tax form adjustment.</summary>
public sealed record RollingTaxFormAdjustmentValidator(List<IRollingTaxFormAdjustmentRule> Rules)
{
    public List<string> Validate(RollingTaxFormAdjustment item)
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

    public void RequireValid(RollingTaxFormAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingTaxFormAdjustment> items)
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

/// <summary>Functions of rolling tax form adjustment rules.</summary>
public static class RollingTaxFormAdjustmentRules
{
    public static RollingTaxFormAdjustmentValidator DefaultRollingTaxFormAdjustmentValidator()
    {
        return new RollingTaxFormAdjustmentValidator(
            new List<IRollingTaxFormAdjustmentRule>
            {
                new RollingTaxFormAdjustmentRequiredCodeRule("rolling tax form adjustment needs a value"),
                new RollingTaxFormAdjustmentNameLengthRule(32),
                new RollingTaxFormAdjustmentPinnedGuardRule(80),
                new RollingTaxFormAdjustmentCapacityRangeRule(64, 86400),
                new RollingTaxFormAdjustmentGradeLengthRule(80),
            });
    }
}
