namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg baseline is in its life cycle.</summary>
public enum ReorgBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a reorg baseline and its current status.</summary>
public sealed class ReorgBaseline(string code, string title, int capacity, string phone, string country, string skill, string timezone, string employeeId, int retries, ReorgBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Phone { get; set; } = phone;
    public string Country { get; set; } = country;
    public string Skill { get; set; } = skill;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public ReorgBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a reorg baseline in status requested.</summary>
    public static ReorgBaseline NewReorgBaseline(string code, string title)
    {
        return new ReorgBaseline(code, title, 0, "", "", "", "", "", 0, ReorgBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgBaselineStatus next)
    {
        switch (Status)
        {
            case ReorgBaselineStatus.Requested:
                return next == ReorgBaselineStatus.Revoked;
            case ReorgBaselineStatus.Granted:
                return next == ReorgBaselineStatus.Revoked || next == ReorgBaselineStatus.Expired;
            case ReorgBaselineStatus.Revoked:
                return next == ReorgBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReorgBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Retries + (double)Capacity, 2);
    }

    public ReorgBaseline Copy()
    {
        return new ReorgBaseline(
            Code,
            Title,
            Capacity,
            Phone,
            Country,
            Skill,
            Timezone,
            EmployeeId,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
