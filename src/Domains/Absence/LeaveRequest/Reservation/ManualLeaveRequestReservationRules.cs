namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual leave request reservation must pass. Check returns its problems, or none.</summary>
public interface IManualLeaveRequestReservationRule
{
    List<string> Check(ManualLeaveRequestReservation item);
}

public sealed record ManualLeaveRequestReservationMaximumRangeRule(double Min, double Max) : IManualLeaveRequestReservationRule
{
    public List<string> Check(ManualLeaveRequestReservation item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestReservationRequiredLabelRule(string Message) : IManualLeaveRequestReservationRule
{
    public List<string> Check(ManualLeaveRequestReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestReservationCodePrefixRule(string Prefix) : IManualLeaveRequestReservationRule
{
    public List<string> Check(ManualLeaveRequestReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestReservationRequiredTitleRule(string Message) : IManualLeaveRequestReservationRule
{
    public List<string> Check(ManualLeaveRequestReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualLeaveRequestReservationAllowedLabelRule(List<string> Allowed) : IManualLeaveRequestReservationRule
{
    public List<string> Check(ManualLeaveRequestReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual leave request reservation.</summary>
public sealed record ManualLeaveRequestReservationValidator(List<IManualLeaveRequestReservationRule> Rules)
{
    public List<string> Validate(ManualLeaveRequestReservation item)
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

    public void RequireValid(ManualLeaveRequestReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLeaveRequestReservation> items)
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

/// <summary>Functions of manual leave request reservation rules.</summary>
public static class ManualLeaveRequestReservationRules
{
    public static ManualLeaveRequestReservationValidator DefaultManualLeaveRequestReservationValidator()
    {
        return new ManualLeaveRequestReservationValidator(
            new List<IManualLeaveRequestReservationRule>
            {
                new ManualLeaveRequestReservationMaximumRangeRule(2.4, 162.2),
                new ManualLeaveRequestReservationRequiredLabelRule("manual leave request reservation needs an entry"),
                new ManualLeaveRequestReservationCodePrefixRule("X"),
                new ManualLeaveRequestReservationRequiredTitleRule("manual leave request reservation needs an entry"),
                new ManualLeaveRequestReservationAllowedLabelRule(new List<string> { "weighted" }),
            });
    }
}
