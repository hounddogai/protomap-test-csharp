namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched span reservation is in its life cycle.</summary>
public enum BatchedSpanReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One batched span reservation, with what the planners need to act on it.</summary>
public sealed class BatchedSpanReservation(string code, string name, int graceDays, int sequence, string employeeId, int expiresAt, List<string> tags, int limit, int batchSize, int version, string description, BatchedSpanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public int ExpiresAt { get; set; } = expiresAt;
    public List<string> Tags { get; set; } = tags;
    public int Limit { get; set; } = limit;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public string Description { get; set; } = description;
    public BatchedSpanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a batched span reservation in status requested.</summary>
    public static BatchedSpanReservation NewBatchedSpanReservation(string code, string name)
    {
        return new BatchedSpanReservation(
            code,
            name,
            0,
            0,
            "",
            0,
            new List<string>(),
            0,
            0,
            0,
            "",
            BatchedSpanReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSpanReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSpanReservationStatus next)
    {
        switch (Status)
        {
            case BatchedSpanReservationStatus.Requested:
                return next == BatchedSpanReservationStatus.Granted || next == BatchedSpanReservationStatus.Revoked;
            case BatchedSpanReservationStatus.Granted:
                switch (next)
                {
                    case BatchedSpanReservationStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedSpanReservationStatus.Revoked:
                switch (next)
                {
                    case BatchedSpanReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedSpanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched span reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Version + (double)ExpiresAt, 2);
    }

    public BatchedSpanReservation Copy()
    {
        return new BatchedSpanReservation(
            Code,
            Name,
            GraceDays,
            Sequence,
            EmployeeId,
            ExpiresAt,
            Tags,
            Limit,
            BatchSize,
            Version,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
