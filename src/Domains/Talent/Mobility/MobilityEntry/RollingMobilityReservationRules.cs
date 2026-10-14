namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling mobility reservation must pass. Check returns its problems, or none.</summary>
public interface IRollingMobilityReservationRule
{
    List<string> Check(RollingMobilityReservation item);
}

public sealed record RollingMobilityReservationCreatedAtRangeRule(int Min, int Max) : IRollingMobilityReservationRule
{
    public List<string> Check(RollingMobilityReservation item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityReservationGraceDaysRangeRule(int Min, int Max) : IRollingMobilityReservationRule
{
    public List<string> Check(RollingMobilityReservation item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityReservationAllowedCodeRule(List<string> Allowed) : IRollingMobilityReservationRule
{
    public List<string> Check(RollingMobilityReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityReservationCeilingRangeRule(double Min, double Max) : IRollingMobilityReservationRule
{
    public List<string> Check(RollingMobilityReservation item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityReservationRequiredCodeRule(string Message) : IRollingMobilityReservationRule
{
    public List<string> Check(RollingMobilityReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling mobility reservation.</summary>
public sealed record RollingMobilityReservationValidator(List<IRollingMobilityReservationRule> Rules)
{
    public List<string> Validate(RollingMobilityReservation item)
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

    public void RequireValid(RollingMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMobilityReservation> items)
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

/// <summary>Functions of rolling mobility reservation rules.</summary>
public static class RollingMobilityReservationRules
{
    public static RollingMobilityReservationValidator DefaultRollingMobilityReservationValidator()
    {
        return new RollingMobilityReservationValidator(
            new List<IRollingMobilityReservationRule>
            {
                new RollingMobilityReservationCreatedAtRangeRule(32, 80),
                new RollingMobilityReservationGraceDaysRangeRule(3600, 86400),
                new RollingMobilityReservationAllowedCodeRule(new List<string>()),
                new RollingMobilityReservationCeilingRangeRule(4.6, 1846.1),
                new RollingMobilityReservationRequiredCodeRule("rolling mobility reservation needs a value"),
            });
    }
}
