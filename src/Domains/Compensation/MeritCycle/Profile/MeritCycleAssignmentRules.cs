namespace Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>A rule a merit cycle assignment must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleAssignmentRule
{
    List<string> Check(MeritCycleAssignment item);
}

public sealed record MeritCycleAssignmentCodeLengthRule(int MaxLength) : IMeritCycleAssignmentRule
{
    public List<string> Check(MeritCycleAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAssignmentSourceLengthRule(int MaxLength) : IMeritCycleAssignmentRule
{
    public List<string> Check(MeritCycleAssignment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleAssignmentQuantityRangeRule(int Min, int Max) : IMeritCycleAssignmentRule
{
    public List<string> Check(MeritCycleAssignment item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle assignment.</summary>
public sealed record MeritCycleAssignmentValidator(List<IMeritCycleAssignmentRule> Rules)
{
    public List<string> Validate(MeritCycleAssignment item)
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

    public void RequireValid(MeritCycleAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleAssignment> items)
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

/// <summary>Functions of merit cycle assignment rules.</summary>
public static class MeritCycleAssignmentRules
{
    public static MeritCycleAssignmentValidator DefaultMeritCycleAssignmentValidator()
    {
        return new MeritCycleAssignmentValidator(
            new List<IMeritCycleAssignmentRule>
            {
                new MeritCycleAssignmentCodeLengthRule(16),
                new MeritCycleAssignmentSourceLengthRule(120),
                new MeritCycleAssignmentQuantityRangeRule(32, 1000),
            });
    }
}
