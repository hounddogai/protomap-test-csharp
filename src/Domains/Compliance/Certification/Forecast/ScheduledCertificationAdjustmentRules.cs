namespace Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>A rule a scheduled certification adjustment must pass. Check returns its problems, or none.</summary>
public interface IScheduledCertificationAdjustmentRule
{
    List<string> Check(ScheduledCertificationAdjustment item);
}

public sealed record ScheduledCertificationAdjustmentRequiredCodeRule(string Message) : IScheduledCertificationAdjustmentRule
{
    public List<string> Check(ScheduledCertificationAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCertificationAdjustmentWeightRangeRule(double Min, double Max) : IScheduledCertificationAdjustmentRule
{
    public List<string> Check(ScheduledCertificationAdjustment item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledCertificationAdjustmentContactEmailLengthRule(int MaxLength) : IScheduledCertificationAdjustmentRule
{
    public List<string> Check(ScheduledCertificationAdjustment item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledCertificationAdjustmentWindowDaysRangeRule(int Min, int Max) : IScheduledCertificationAdjustmentRule
{
    public List<string> Check(ScheduledCertificationAdjustment item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled certification adjustment.</summary>
public sealed record ScheduledCertificationAdjustmentValidator(List<IScheduledCertificationAdjustmentRule> Rules)
{
    public List<string> Validate(ScheduledCertificationAdjustment item)
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

    public void RequireValid(ScheduledCertificationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledCertificationAdjustment> items)
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

/// <summary>Functions of scheduled certification adjustment rules.</summary>
public static class ScheduledCertificationAdjustmentRules
{
    public static ScheduledCertificationAdjustmentValidator DefaultScheduledCertificationAdjustmentValidator()
    {
        return new ScheduledCertificationAdjustmentValidator(
            new List<IScheduledCertificationAdjustmentRule>
            {
                new ScheduledCertificationAdjustmentRequiredCodeRule(
                    "scheduled certification adjustment needs an entry"),
                new ScheduledCertificationAdjustmentWeightRangeRule(6.5, 668.3),
                new ScheduledCertificationAdjustmentContactEmailLengthRule(80),
                new ScheduledCertificationAdjustmentWindowDaysRangeRule(16, 32),
            });
    }
}
