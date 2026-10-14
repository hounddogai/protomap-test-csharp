namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped cost model assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedCostModelAssignmentRule
{
    List<string> Check(CappedCostModelAssignment item);
}

public sealed record CappedCostModelAssignmentNameLengthRule(int MaxLength) : ICappedCostModelAssignmentRule
{
    public List<string> Check(CappedCostModelAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelAssignmentAllowedChannelRule(List<string> Allowed) : ICappedCostModelAssignmentRule
{
    public List<string> Check(CappedCostModelAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record CappedCostModelAssignmentCodeLengthRule(int MaxLength) : ICappedCostModelAssignmentRule
{
    public List<string> Check(CappedCostModelAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped cost model assignment.</summary>
public sealed record CappedCostModelAssignmentValidator(List<ICappedCostModelAssignmentRule> Rules)
{
    public List<string> Validate(CappedCostModelAssignment item)
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

    public void RequireValid(CappedCostModelAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCostModelAssignment> items)
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

/// <summary>Functions of capped cost model assignment rules.</summary>
public static class CappedCostModelAssignmentRules
{
    public static CappedCostModelAssignmentValidator DefaultCappedCostModelAssignmentValidator()
    {
        return new CappedCostModelAssignmentValidator(
            new List<ICappedCostModelAssignmentRule>
            {
                new CappedCostModelAssignmentNameLengthRule(32),
                new CappedCostModelAssignmentAllowedChannelRule(new List<string>()),
                new CappedCostModelAssignmentCodeLengthRule(64),
            });
    }
}
