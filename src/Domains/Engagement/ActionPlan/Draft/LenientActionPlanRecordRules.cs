namespace Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>A rule a lenient action plan record must pass. Check returns its problems, or none.</summary>
public interface ILenientActionPlanRecordRule
{
    List<string> Check(LenientActionPlanRecord item);
}

public sealed record LenientActionPlanRecordCapacityRangeRule(int Min, int Max) : ILenientActionPlanRecordRule
{
    public List<string> Check(LenientActionPlanRecord item)
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

public sealed record LenientActionPlanRecordRequiredPhoneRule(string Message) : ILenientActionPlanRecordRule
{
    public List<string> Check(LenientActionPlanRecord item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanRecordRequiredCodeRule(string Message) : ILenientActionPlanRecordRule
{
    public List<string> Check(LenientActionPlanRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient action plan record.</summary>
public sealed record LenientActionPlanRecordValidator(List<ILenientActionPlanRecordRule> Rules)
{
    public List<string> Validate(LenientActionPlanRecord item)
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

    public void RequireValid(LenientActionPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientActionPlanRecord> items)
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

/// <summary>Functions of lenient action plan record rules.</summary>
public static class LenientActionPlanRecordRules
{
    public static LenientActionPlanRecordValidator DefaultLenientActionPlanRecordValidator()
    {
        return new LenientActionPlanRecordValidator(
            new List<ILenientActionPlanRecordRule>
            {
                new LenientActionPlanRecordCapacityRangeRule(16, 120),
                new LenientActionPlanRecordRequiredPhoneRule("lenient action plan record needs a value"),
                new LenientActionPlanRecordRequiredCodeRule("lenient action plan record needs a value"),
            });
    }
}
