namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped accrual score is in its life cycle.</summary>
public enum CappedAccrualScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped accrual score, with what the planners need to act on it.</summary>
public sealed class CappedAccrualScore(string code, string label, int retries, int startsAt, double floorValue, int batchSize, int revision, int endsAt, string segment, double rating, CappedAccrualScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int StartsAt { get; set; } = startsAt;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public int Revision { get; set; } = revision;
    public int EndsAt { get; set; } = endsAt;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public CappedAccrualScoreStatus Status { get; set; } = status;

    /// <summary>Starts a capped accrual score in status proposed.</summary>
    public static CappedAccrualScore NewCappedAccrualScore(string code, string label)
    {
        return new CappedAccrualScore(code, label, 0, 0, 0.0, 0, 0, 0, "", 0.0, CappedAccrualScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccrualScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccrualScoreStatus next)
    {
        switch (Status)
        {
            case CappedAccrualScoreStatus.Proposed:
                switch (next)
                {
                    case CappedAccrualScoreStatus.Approved:
                        return true;
                }
                return false;
            case CappedAccrualScoreStatus.Approved:
                switch (next)
                {
                    case CappedAccrualScoreStatus.Rejected:
                        return true;
                }
                return false;
            case CappedAccrualScoreStatus.Rejected:
                return next == CappedAccrualScoreStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedAccrualScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped accrual score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt - (double)BatchSize, 2);
    }

    public CappedAccrualScore Copy()
    {
        return new CappedAccrualScore(
            Code,
            Label,
            Retries,
            StartsAt,
            FloorValue,
            BatchSize,
            Revision,
            EndsAt,
            Segment,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
