namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan record must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanRecordRule
{
    List<string> Check(SuccessionPlanRecord item);
}

public sealed record SuccessionPlanRecordNameLengthRule(int MaxLength) : ISuccessionPlanRecordRule
{
    public List<string> Check(SuccessionPlanRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRecordStartsAtRangeRule(int Min, int Max) : ISuccessionPlanRecordRule
{
    public List<string> Check(SuccessionPlanRecord item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanRecordAllowedEmployeeIdRule(List<string> Allowed) : ISuccessionPlanRecordRule
{
    public List<string> Check(SuccessionPlanRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan record.</summary>
public sealed record SuccessionPlanRecordValidator(List<ISuccessionPlanRecordRule> Rules)
{
    public List<string> Validate(SuccessionPlanRecord item)
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

    public void RequireValid(SuccessionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanRecord> items)
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

/// <summary>Functions of succession plan record rules.</summary>
public static class SuccessionPlanRecordRules
{
    public static SuccessionPlanRecordValidator DefaultSuccessionPlanRecordValidator()
    {
        return new SuccessionPlanRecordValidator(
            new List<ISuccessionPlanRecordRule>
            {
                new SuccessionPlanRecordNameLengthRule(86400),
                new SuccessionPlanRecordStartsAtRangeRule(255, 86400),
                new SuccessionPlanRecordAllowedEmployeeIdRule(new List<string> { "capped", "rolling", "batched" }),
            });
    }
}
