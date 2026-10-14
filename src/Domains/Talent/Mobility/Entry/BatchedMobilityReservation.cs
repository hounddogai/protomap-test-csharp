namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched mobility reservation is in its life cycle.</summary>
public enum BatchedMobilityReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a batched mobility reservation and its current status.</summary>
public sealed class BatchedMobilityReservation(string code, string title, List<string> tags, int windowDays, double maximum, string locale, int batchSize, BatchedMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public double Maximum { get; set; } = maximum;
    public string Locale { get; set; } = locale;
    public int BatchSize { get; set; } = batchSize;
    public BatchedMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a batched mobility reservation in status requested.</summary>
    public static BatchedMobilityReservation NewBatchedMobilityReservation(string code, string title)
    {
        return new BatchedMobilityReservation(
            code,
            title,
            new List<string>(),
            0,
            0.0,
            "",
            0,
            BatchedMobilityReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMobilityReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMobilityReservationStatus next)
    {
        switch (Status)
        {
            case BatchedMobilityReservationStatus.Requested:
                switch (next)
                {
                    case BatchedMobilityReservationStatus.Granted:
                    case BatchedMobilityReservationStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedMobilityReservationStatus.Granted:
                switch (next)
                {
                    case BatchedMobilityReservationStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedMobilityReservationStatus.Revoked:
                switch (next)
                {
                    case BatchedMobilityReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetTotal()
    {
        return Math.Round(Maximum * (double)WindowDays, 2);
    }

    public BatchedMobilityReservation Copy()
    {
        return new BatchedMobilityReservation(Code, Title, Tags, WindowDays, Maximum, Locale, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
