namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run record is in its life cycle.</summary>
public enum PayRunRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pay run record, with what the planners need to act on it.</summary>
public sealed class PayRunRecord(string code, string name, double weight, double floorValue, int expiresAt, bool active, double rate, PayRunRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Active { get; set; } = active;
    public double Rate { get; set; } = rate;
    public PayRunRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pay run record in status open.</summary>
    public static PayRunRecord NewPayRunRecord(string code, string name)
    {
        return new PayRunRecord(code, name, 0.0, 0.0, 0, false, 0.0, PayRunRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunRecordStatus next)
    {
        switch (Status)
        {
            case PayRunRecordStatus.Open:
                return next == PayRunRecordStatus.Resolved;
            case PayRunRecordStatus.InReview:
                return next == PayRunRecordStatus.Reopened;
            case PayRunRecordStatus.Resolved:
                return next == PayRunRecordStatus.Reopened || next == PayRunRecordStatus.Archived;
            case PayRunRecordStatus.Reopened:
                switch (next)
                {
                    case PayRunRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue * Weight, 2);
    }

    public PayRunRecord Copy()
    {
        return new PayRunRecord(Code, Name, Weight, FloorValue, ExpiresAt, Active, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
