namespace Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>A rule a visitor schedule must pass. Check returns its problems, or none.</summary>
public interface IVisitorScheduleRule
{
    List<string> Check(VisitorSchedule item);
}

public sealed record VisitorScheduleSalaryBandLengthRule(int MaxLength) : IVisitorScheduleRule
{
    public List<string> Check(VisitorSchedule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorScheduleLabelLengthRule(int MaxLength) : IVisitorScheduleRule
{
    public List<string> Check(VisitorSchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorScheduleRequiredLabelRule(string Message) : IVisitorScheduleRule
{
    public List<string> Check(VisitorSchedule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor schedule.</summary>
public sealed record VisitorScheduleValidator(List<IVisitorScheduleRule> Rules)
{
    public List<string> Validate(VisitorSchedule item)
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

    public void RequireValid(VisitorSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorSchedule> items)
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

/// <summary>Functions of visitor schedule rules.</summary>
public static class VisitorScheduleRules
{
    public static VisitorScheduleValidator DefaultVisitorScheduleValidator()
    {
        return new VisitorScheduleValidator(
            new List<IVisitorScheduleRule>
            {
                new VisitorScheduleSalaryBandLengthRule(120),
                new VisitorScheduleLabelLengthRule(16),
                new VisitorScheduleRequiredLabelRule("visitor schedule needs this field"),
            });
    }
}
