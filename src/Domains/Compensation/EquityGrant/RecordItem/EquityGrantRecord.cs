namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant record is in its life cycle.</summary>
public enum EquityGrantRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A equity grant record. Its code is unique within a tenant.</summary>
public sealed class EquityGrantRecord(string code, string label, int endsAt, double ratio, double score, string contactEmail, int startsAt, EquityGrantRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public EquityGrantRecordStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant record in status proposed.</summary>
    public static EquityGrantRecord NewEquityGrantRecord(string code, string label)
    {
        return new EquityGrantRecord(code, label, 0, 0.0, 0.0, "", 0, EquityGrantRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantRecordStatus next)
    {
        switch (Status)
        {
            case EquityGrantRecordStatus.Proposed:
                return next == EquityGrantRecordStatus.Rejected || next == EquityGrantRecordStatus.Withdrawn;
            case EquityGrantRecordStatus.Approved:
                return next == EquityGrantRecordStatus.Rejected;
            case EquityGrantRecordStatus.Rejected:
                return next == EquityGrantRecordStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(EquityGrantRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)StartsAt * Score, 2);
    }

    public EquityGrantRecord Copy()
    {
        return new EquityGrantRecord(Code, Label, EndsAt, Ratio, Score, ContactEmail, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
