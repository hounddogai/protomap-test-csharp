namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run policy is in its life cycle.</summary>
public enum PayRunPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pay run policy as the pay run team keeps it.</summary>
public sealed class PayRunPolicy(string code, string label, double minimum, string skill, int leaveDays, double floorValue, string externalRef, string locale, string source, string location, string team, PayRunPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public string ExternalRef { get; set; } = externalRef;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public string Team { get; set; } = team;
    public PayRunPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pay run policy in status planned.</summary>
    public static PayRunPolicy NewPayRunPolicy(string code, string label)
    {
        return new PayRunPolicy(code, label, 0.0, "", 0, 0.0, "", "", "", "", "", PayRunPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunPolicyStatus next)
    {
        switch (Status)
        {
            case PayRunPolicyStatus.Planned:
                switch (next)
                {
                    case PayRunPolicyStatus.Completed:
                        return true;
                }
                return false;
            case PayRunPolicyStatus.Scheduled:
                return next == PayRunPolicyStatus.InProgress;
            case PayRunPolicyStatus.InProgress:
                return next == PayRunPolicyStatus.Cancelled;
            case PayRunPolicyStatus.Completed:
                switch (next)
                {
                    case PayRunPolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum * (double)LeaveDays, 2);
    }

    public PayRunPolicy Copy()
    {
        return new PayRunPolicy(
            Code,
            Label,
            Minimum,
            Skill,
            LeaveDays,
            FloorValue,
            ExternalRef,
            Locale,
            Source,
            Location,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
