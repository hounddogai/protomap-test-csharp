namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band snapshot is in its life cycle.</summary>
public enum PayBandSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pay band snapshot as the pay band team keeps it.</summary>
public sealed class PayBandSnapshot(string code, string title, string category, string badge, int quantity, string skill, string jobCode, PayBandSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public string Skill { get; set; } = skill;
    public string JobCode { get; set; } = jobCode;
    public PayBandSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a pay band snapshot in status draft.</summary>
    public static PayBandSnapshot NewPayBandSnapshot(string code, string title)
    {
        return new PayBandSnapshot(code, title, "", "", 0, "", "", PayBandSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandSnapshotStatus next)
    {
        switch (Status)
        {
            case PayBandSnapshotStatus.Draft:
                switch (next)
                {
                    case PayBandSnapshotStatus.Pending:
                        return true;
                }
                return false;
            case PayBandSnapshotStatus.Pending:
                return next == PayBandSnapshotStatus.Active || next == PayBandSnapshotStatus.Suspended;
            case PayBandSnapshotStatus.Active:
                return next == PayBandSnapshotStatus.Closed;
            case PayBandSnapshotStatus.Suspended:
                return next == PayBandSnapshotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayBandSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PayBandSnapshot Copy()
    {
        return new PayBandSnapshot(Code, Title, Category, Badge, Quantity, Skill, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
