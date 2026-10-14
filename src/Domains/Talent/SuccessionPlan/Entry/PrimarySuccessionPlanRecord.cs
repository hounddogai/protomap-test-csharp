namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary succession plan record is in its life cycle.</summary>
public enum PrimarySuccessionPlanRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary succession plan record. Its code is unique within a tenant.</summary>
public sealed class PrimarySuccessionPlanRecord(string code, string name, string region, int durationMinutes, int leaveDays, string location, double weight, string notes, PrimarySuccessionPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string Notes { get; set; } = notes;
    public PrimarySuccessionPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a primary succession plan record in status requested.</summary>
    public static PrimarySuccessionPlanRecord NewPrimarySuccessionPlanRecord(string code, string name)
    {
        return new PrimarySuccessionPlanRecord(
            code,
            name,
            "",
            0,
            0,
            "",
            0.0,
            "",
            PrimarySuccessionPlanRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySuccessionPlanRecordStatus next)
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRecordStatus.Requested:
                return next == PrimarySuccessionPlanRecordStatus.Granted || next == PrimarySuccessionPlanRecordStatus.Revoked;
            case PrimarySuccessionPlanRecordStatus.Granted:
                switch (next)
                {
                    case PrimarySuccessionPlanRecordStatus.Revoked:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanRecordStatus.Revoked:
                return next == PrimarySuccessionPlanRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimarySuccessionPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary succession plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round((double)DurationMinutes - (double)LeaveDays, 2);
    }

    public PrimarySuccessionPlanRecord Copy()
    {
        return new PrimarySuccessionPlanRecord(
            Code,
            Name,
            Region,
            DurationMinutes,
            LeaveDays,
            Location,
            Weight,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
