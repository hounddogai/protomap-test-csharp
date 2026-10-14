namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped exception checkpoint is in its life cycle.</summary>
public enum CappedExceptionCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a capped exception checkpoint and its current status.</summary>
public sealed class CappedExceptionCheckpoint(string code, string title, double weight, string employeeId, double rating, int attempts, double floorValue, int quantity, string checksum, string timezone, CappedExceptionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string EmployeeId { get; set; } = employeeId;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public double FloorValue { get; set; } = floorValue;
    public int Quantity { get; set; } = quantity;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public CappedExceptionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a capped exception checkpoint in status requested.</summary>
    public static CappedExceptionCheckpoint NewCappedExceptionCheckpoint(string code, string title)
    {
        return new CappedExceptionCheckpoint(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            0,
            "",
            "",
            CappedExceptionCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedExceptionCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedExceptionCheckpointStatus next)
    {
        switch (Status)
        {
            case CappedExceptionCheckpointStatus.Requested:
                switch (next)
                {
                    case CappedExceptionCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case CappedExceptionCheckpointStatus.Granted:
                return next == CappedExceptionCheckpointStatus.Revoked;
            case CappedExceptionCheckpointStatus.Revoked:
                switch (next)
                {
                    case CappedExceptionCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedExceptionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped exception checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped exception checkpoint"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity - FloorValue, 2);
    }

    public CappedExceptionCheckpoint Copy()
    {
        return new CappedExceptionCheckpoint(
            Code,
            Title,
            Weight,
            EmployeeId,
            Rating,
            Attempts,
            FloorValue,
            Quantity,
            Checksum,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
