namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>A rule a succession plan proposal must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanProposalRule
{
    List<string> Check(SuccessionPlanProposal item);
}

public sealed record SuccessionPlanProposalCapacityRangeRule(int Min, int Max) : ISuccessionPlanProposalRule
{
    public List<string> Check(SuccessionPlanProposal item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanProposalFloorValueRangeRule(double Min, double Max) : ISuccessionPlanProposalRule
{
    public List<string> Check(SuccessionPlanProposal item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanProposalRequiredManagerIdRule(string Message) : ISuccessionPlanProposalRule
{
    public List<string> Check(SuccessionPlanProposal item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanProposalNotesLengthRule(int MaxLength) : ISuccessionPlanProposalRule
{
    public List<string> Check(SuccessionPlanProposal item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanProposalManagerIdLengthRule(int MaxLength) : ISuccessionPlanProposalRule
{
    public List<string> Check(SuccessionPlanProposal item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan proposal.</summary>
public sealed record SuccessionPlanProposalValidator(List<ISuccessionPlanProposalRule> Rules)
{
    public List<string> Validate(SuccessionPlanProposal item)
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

    public void RequireValid(SuccessionPlanProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanProposal> items)
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

/// <summary>Functions of succession plan proposal rules.</summary>
public static class SuccessionPlanProposalRules
{
    public static SuccessionPlanProposalValidator DefaultSuccessionPlanProposalValidator()
    {
        return new SuccessionPlanProposalValidator(
            new List<ISuccessionPlanProposalRule>
            {
                new SuccessionPlanProposalCapacityRangeRule(64, 120),
                new SuccessionPlanProposalFloorValueRangeRule(3.3, 4571.5),
                new SuccessionPlanProposalRequiredManagerIdRule("succession plan proposal needs an entry"),
                new SuccessionPlanProposalNotesLengthRule(16),
                new SuccessionPlanProposalManagerIdLengthRule(3600),
            });
    }
}
