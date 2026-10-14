namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band schedule must pass. Check returns its problems, or none.</summary>
public interface IPayBandScheduleRule
{
    List<string> Check(PayBandSchedule item);
}

public sealed record PayBandScheduleCodePrefixRule(string Prefix) : IPayBandScheduleRule
{
    public List<string> Check(PayBandSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandScheduleAllowedLabelRule(List<string> Allowed) : IPayBandScheduleRule
{
    public List<string> Check(PayBandSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PayBandScheduleDescriptionLengthRule(int MaxLength) : IPayBandScheduleRule
{
    public List<string> Check(PayBandSchedule item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandScheduleFloorValueRangeRule(double Min, double Max) : IPayBandScheduleRule
{
    public List<string> Check(PayBandSchedule item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band schedule.</summary>
public sealed record PayBandScheduleValidator(List<IPayBandScheduleRule> Rules)
{
    public List<string> Validate(PayBandSchedule item)
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

    public void RequireValid(PayBandSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandSchedule> items)
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

/// <summary>Functions of pay band schedule rules.</summary>
public static class PayBandScheduleRules
{
    public static PayBandScheduleValidator DefaultPayBandScheduleValidator()
    {
        return new PayBandScheduleValidator(
            new List<IPayBandScheduleRule>
            {
                new PayBandScheduleCodePrefixRule("ACM"),
                new PayBandScheduleAllowedLabelRule(new List<string> { "primary", "scheduled", "primary" }),
                new PayBandScheduleDescriptionLengthRule(32),
                new PayBandScheduleFloorValueRangeRule(2.3, 2148.1),
            });
    }
}
