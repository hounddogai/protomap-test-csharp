namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band milestone must pass. Check returns its problems, or none.</summary>
public interface IPayBandMilestoneRule
{
    List<string> Check(PayBandMilestone item);
}

public sealed record PayBandMilestoneAllowedTitleRule(List<string> Allowed) : IPayBandMilestoneRule
{
    public List<string> Check(PayBandMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMilestoneMarginRangeRule(double Min, double Max) : IPayBandMilestoneRule
{
    public List<string> Check(PayBandMilestone item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandMilestoneCodeLengthRule(int MaxLength) : IPayBandMilestoneRule
{
    public List<string> Check(PayBandMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band milestone.</summary>
public sealed record PayBandMilestoneValidator(List<IPayBandMilestoneRule> Rules)
{
    public List<string> Validate(PayBandMilestone item)
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

    public void RequireValid(PayBandMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandMilestone> items)
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

/// <summary>Functions of pay band milestone rules.</summary>
public static class PayBandMilestoneRules
{
    public static PayBandMilestoneValidator DefaultPayBandMilestoneValidator()
    {
        return new PayBandMilestoneValidator(
            new List<IPayBandMilestoneRule>
            {
                new PayBandMilestoneAllowedTitleRule(new List<string> { "lenient", "seasonal" }),
                new PayBandMilestoneMarginRangeRule(1.9, 2032.8),
                new PayBandMilestoneCodeLengthRule(120),
            });
    }
}
