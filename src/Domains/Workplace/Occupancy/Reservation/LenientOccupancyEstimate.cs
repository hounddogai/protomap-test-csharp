namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient occupancy estimate is in its life cycle.</summary>
public enum LenientOccupancyEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient occupancy estimate, with what the planners need to act on it.</summary>
public sealed class LenientOccupancyEstimate(string code, string name, string label, int leaveDays, string managerId, int sequence, string checksum, string timezone, LenientOccupancyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public LenientOccupancyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient occupancy estimate in status requested.</summary>
    public static LenientOccupancyEstimate NewLenientOccupancyEstimate(string code, string name)
    {
        return new LenientOccupancyEstimate(
            code,
            name,
            "",
            0,
            "",
            0,
            "",
            "",
            LenientOccupancyEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientOccupancyEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientOccupancyEstimateStatus next)
    {
        switch (Status)
        {
            case LenientOccupancyEstimateStatus.Requested:
                return next == LenientOccupancyEstimateStatus.Granted || next == LenientOccupancyEstimateStatus.Revoked;
            case LenientOccupancyEstimateStatus.Granted:
                switch (next)
                {
                    case LenientOccupancyEstimateStatus.Revoked:
                    case LenientOccupancyEstimateStatus.Expired:
                        return true;
                }
                return false;
            case LenientOccupancyEstimateStatus.Revoked:
                switch (next)
                {
                    case LenientOccupancyEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientOccupancyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient occupancy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays - (double)Sequence, 2);
    }

    public LenientOccupancyEstimate Copy()
    {
        return new LenientOccupancyEstimate(
            Code,
            Name,
            Label,
            LeaveDays,
            ManagerId,
            Sequence,
            Checksum,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
