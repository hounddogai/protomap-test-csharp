namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat allocation is in its life cycle.</summary>
public enum LicenseSeatAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a license seat allocation and its current status.</summary>
public sealed class LicenseSeatAllocation(string code, string label, double ceiling, int limit, int quantity, int capacity, int startsAt, int attempts, double discount, int updatedAt, string source, LicenseSeatAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Source { get; set; } = source;
    public LicenseSeatAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a license seat allocation in status planned.</summary>
    public static LicenseSeatAllocation NewLicenseSeatAllocation(string code, string label)
    {
        return new LicenseSeatAllocation(
            code,
            label,
            0.0,
            0,
            0,
            0,
            0,
            0,
            0.0,
            0,
            "",
            LicenseSeatAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatAllocationStatus next)
    {
        switch (Status)
        {
            case LicenseSeatAllocationStatus.Planned:
                switch (next)
                {
                    case LicenseSeatAllocationStatus.Completed:
                        return true;
                }
                return false;
            case LicenseSeatAllocationStatus.Scheduled:
                switch (next)
                {
                    case LicenseSeatAllocationStatus.Completed:
                    case LicenseSeatAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case LicenseSeatAllocationStatus.InProgress:
                switch (next)
                {
                    case LicenseSeatAllocationStatus.Completed:
                    case LicenseSeatAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case LicenseSeatAllocationStatus.Completed:
                switch (next)
                {
                    case LicenseSeatAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(Ceiling * (double)Limit, 2);
    }

    public LicenseSeatAllocation Copy()
    {
        return new LicenseSeatAllocation(
            Code,
            Label,
            Ceiling,
            Limit,
            Quantity,
            Capacity,
            StartsAt,
            Attempts,
            Discount,
            UpdatedAt,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
