namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual mobility reservation must pass. Check returns its problems, or none.</summary>
public interface IManualMobilityReservationRule
{
    List<string> Check(ManualMobilityReservation item);
}

public sealed record ManualMobilityReservationRequiredCodeRule(string Message) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityReservationRequiredTitleRule(string Message) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityReservationMarginRangeRule(double Min, double Max) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
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

public sealed record ManualMobilityReservationMaximumRangeRule(double Min, double Max) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualMobilityReservationGradeLengthRule(int MaxLength) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualMobilityReservationAllowedManagerIdRule(List<string> Allowed) : IManualMobilityReservationRule
{
    public List<string> Check(ManualMobilityReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual mobility reservation.</summary>
public sealed record ManualMobilityReservationValidator(List<IManualMobilityReservationRule> Rules)
{
    public List<string> Validate(ManualMobilityReservation item)
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

    public void RequireValid(ManualMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualMobilityReservation> items)
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

/// <summary>Functions of manual mobility reservation rules.</summary>
public static class ManualMobilityReservationRules
{
    public static ManualMobilityReservationValidator DefaultManualMobilityReservationValidator()
    {
        return new ManualMobilityReservationValidator(
            new List<IManualMobilityReservationRule>
            {
                new ManualMobilityReservationRequiredCodeRule("manual mobility reservation needs this field"),
                new ManualMobilityReservationRequiredTitleRule("manual mobility reservation needs this field"),
                new ManualMobilityReservationMarginRangeRule(6.0, 2605.4),
                new ManualMobilityReservationMaximumRangeRule(3.9, 1640.7),
                new ManualMobilityReservationGradeLengthRule(16),
                new ManualMobilityReservationAllowedManagerIdRule(new List<string> { "fallback" }),
            });
    }
}
