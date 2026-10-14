namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan schedule must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanScheduleRule
{
    List<string> Check(SuccessionPlanSchedule item);
}

public sealed record SuccessionPlanScheduleSequenceRangeRule(int Min, int Max) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScheduleAllowedLabelRule(List<string> Allowed) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScheduleRequiredCodeRule(string Message) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScheduleCodeLengthRule(int MaxLength) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScheduleRequiredLabelRule(string Message) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanScheduleMarginRangeRule(double Min, double Max) : ISuccessionPlanScheduleRule
{
    public List<string> Check(SuccessionPlanSchedule item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a succession plan schedule.</summary>
public sealed record SuccessionPlanScheduleValidator(List<ISuccessionPlanScheduleRule> Rules)
{
    public List<string> Validate(SuccessionPlanSchedule item)
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

    public void RequireValid(SuccessionPlanSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanSchedule> items)
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

/// <summary>Functions of succession plan schedule rules.</summary>
public static class SuccessionPlanScheduleRules
{
    public static SuccessionPlanScheduleValidator DefaultSuccessionPlanScheduleValidator()
    {
        return new SuccessionPlanScheduleValidator(
            new List<ISuccessionPlanScheduleRule>
            {
                new SuccessionPlanScheduleSequenceRangeRule(16, 64),
                new SuccessionPlanScheduleAllowedLabelRule(new List<string> { "scheduled", "strict" }),
                new SuccessionPlanScheduleRequiredCodeRule("succession plan schedule needs this field"),
                new SuccessionPlanScheduleCodeLengthRule(80),
                new SuccessionPlanScheduleRequiredLabelRule("succession plan schedule needs this field"),
                new SuccessionPlanScheduleMarginRangeRule(0.6, 1042.7),
            });
    }
}
