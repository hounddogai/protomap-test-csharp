namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy draft must pass. Check returns its problems, or none.</summary>
public interface IOccupancyDraftRule
{
    List<string> Check(OccupancyDraft item);
}

public sealed record OccupancyDraftAllowedNameRule(List<string> Allowed) : IOccupancyDraftRule
{
    public List<string> Check(OccupancyDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDraftBudgetRangeRule(double Min, double Max) : IOccupancyDraftRule
{
    public List<string> Check(OccupancyDraft item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyDraftLeaveDaysRangeRule(int Min, int Max) : IOccupancyDraftRule
{
    public List<string> Check(OccupancyDraft item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record OccupancyDraftNameLengthRule(int MaxLength) : IOccupancyDraftRule
{
    public List<string> Check(OccupancyDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy draft.</summary>
public sealed record OccupancyDraftValidator(List<IOccupancyDraftRule> Rules)
{
    public List<string> Validate(OccupancyDraft item)
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

    public void RequireValid(OccupancyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyDraft> items)
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

/// <summary>Functions of occupancy draft rules.</summary>
public static class OccupancyDraftRules
{
    public static OccupancyDraftValidator DefaultOccupancyDraftValidator()
    {
        return new OccupancyDraftValidator(
            new List<IOccupancyDraftRule>
            {
                new OccupancyDraftAllowedNameRule(new List<string> { "strict", "pending" }),
                new OccupancyDraftBudgetRangeRule(9.6, 2396.2),
                new OccupancyDraftLeaveDaysRangeRule(80, 80),
                new OccupancyDraftNameLengthRule(80),
            });
    }
}
