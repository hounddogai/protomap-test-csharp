namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched nomination reservation is in its life cycle.</summary>
public enum BatchedNominationReservationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched nomination reservation. Its code is unique within a tenant.</summary>
public sealed class BatchedNominationReservation(string code, string title, List<string> tags, bool approved, int updatedAt, string label, BatchedNominationReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public bool Approved { get; set; } = approved;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Label { get; set; } = label;
    public BatchedNominationReservationStatus Status { get; set; } = status;

    /// <summary>Starts a batched nomination reservation in status new.</summary>
    public static BatchedNominationReservation NewBatchedNominationReservation(string code, string title)
    {
        return new BatchedNominationReservation(
            code,
            title,
            new List<string>(),
            false,
            0,
            "",
            BatchedNominationReservationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedNominationReservationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedNominationReservationStatus next)
    {
        switch (Status)
        {
            case BatchedNominationReservationStatus.New:
                return next == BatchedNominationReservationStatus.Failed;
            case BatchedNominationReservationStatus.Queued:
                switch (next)
                {
                    case BatchedNominationReservationStatus.Done:
                        return true;
                }
                return false;
            case BatchedNominationReservationStatus.Running:
                switch (next)
                {
                    case BatchedNominationReservationStatus.Failed:
                        return true;
                }
                return false;
            case BatchedNominationReservationStatus.Done:
                return next == BatchedNominationReservationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedNominationReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched nomination reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
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

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public BatchedNominationReservation Copy()
    {
        return new BatchedNominationReservation(Code, Title, Tags, Approved, UpdatedAt, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
