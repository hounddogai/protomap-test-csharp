namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary timesheet adjustment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryTimesheetAdjustmentRule
{
    List<string> Check(PrimaryTimesheetAdjustment item);
}

public sealed record PrimaryTimesheetAdjustmentTitleLengthRule(int MaxLength) : IPrimaryTimesheetAdjustmentRule
{
    public List<string> Check(PrimaryTimesheetAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetAdjustmentAllowedUnitRule(List<string> Allowed) : IPrimaryTimesheetAdjustmentRule
{
    public List<string> Check(PrimaryTimesheetAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryTimesheetAdjustmentCostRangeRule(double Min, double Max) : IPrimaryTimesheetAdjustmentRule
{
    public List<string> Check(PrimaryTimesheetAdjustment item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryTimesheetAdjustmentSalaryBandLengthRule(int MaxLength) : IPrimaryTimesheetAdjustmentRule
{
    public List<string> Check(PrimaryTimesheetAdjustment item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary timesheet adjustment.</summary>
public sealed record PrimaryTimesheetAdjustmentValidator(List<IPrimaryTimesheetAdjustmentRule> Rules)
{
    public List<string> Validate(PrimaryTimesheetAdjustment item)
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

    public void RequireValid(PrimaryTimesheetAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryTimesheetAdjustment> items)
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

/// <summary>Functions of primary timesheet adjustment rules.</summary>
public static class PrimaryTimesheetAdjustmentRules
{
    public static PrimaryTimesheetAdjustmentValidator DefaultPrimaryTimesheetAdjustmentValidator()
    {
        return new PrimaryTimesheetAdjustmentValidator(
            new List<IPrimaryTimesheetAdjustmentRule>
            {
                new PrimaryTimesheetAdjustmentTitleLengthRule(32),
                new PrimaryTimesheetAdjustmentAllowedUnitRule(new List<string>()),
                new PrimaryTimesheetAdjustmentCostRangeRule(0.9, 3432.1),
                new PrimaryTimesheetAdjustmentSalaryBandLengthRule(86400),
            });
    }
}
