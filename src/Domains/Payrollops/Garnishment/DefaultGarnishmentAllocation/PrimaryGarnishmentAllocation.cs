namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary garnishment allocation is in its life cycle.</summary>
public enum PrimaryGarnishmentAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary garnishment allocation. Its code is unique within a tenant.</summary>
public sealed class PrimaryGarnishmentAllocation(string code, string label, string channel, string reviewer, double ratio, string contactEmail, string skill, int durationMinutes, string description, double maximum, string source, PrimaryGarnishmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public string Source { get; set; } = source;
    public PrimaryGarnishmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a primary garnishment allocation in status requested.</summary>
    public static PrimaryGarnishmentAllocation NewPrimaryGarnishmentAllocation(string code, string label)
    {
        return new PrimaryGarnishmentAllocation(
            code,
            label,
            "",
            "",
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            PrimaryGarnishmentAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryGarnishmentAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryGarnishmentAllocationStatus next)
    {
        switch (Status)
        {
            case PrimaryGarnishmentAllocationStatus.Requested:
                return next == PrimaryGarnishmentAllocationStatus.Granted;
            case PrimaryGarnishmentAllocationStatus.Granted:
                return next == PrimaryGarnishmentAllocationStatus.Revoked;
            case PrimaryGarnishmentAllocationStatus.Revoked:
                return next == PrimaryGarnishmentAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryGarnishmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary garnishment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes + Ratio, 2);
    }

    public PrimaryGarnishmentAllocation Copy()
    {
        return new PrimaryGarnishmentAllocation(
            Code,
            Label,
            Channel,
            Reviewer,
            Ratio,
            ContactEmail,
            Skill,
            DurationMinutes,
            Description,
            Maximum,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
