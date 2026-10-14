namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling access badge batch is in its life cycle.</summary>
public enum RollingAccessBadgeBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling access badge batch as the access badge team keeps it.</summary>
public sealed class RollingAccessBadgeBatch(string code, string title, string label, int quantity, double hours, int endsAt, string costCenter, string department, RollingAccessBadgeBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public int EndsAt { get; set; } = endsAt;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public RollingAccessBadgeBatchStatus Status { get; set; } = status;

    /// <summary>Starts a rolling access badge batch in status open.</summary>
    public static RollingAccessBadgeBatch NewRollingAccessBadgeBatch(string code, string title)
    {
        return new RollingAccessBadgeBatch(code, title, "", 0, 0.0, 0, "", "", RollingAccessBadgeBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccessBadgeBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccessBadgeBatchStatus next)
    {
        switch (Status)
        {
            case RollingAccessBadgeBatchStatus.Open:
                switch (next)
                {
                    case RollingAccessBadgeBatchStatus.Archived:
                        return true;
                }
                return false;
            case RollingAccessBadgeBatchStatus.InReview:
                switch (next)
                {
                    case RollingAccessBadgeBatchStatus.Resolved:
                        return true;
                }
                return false;
            case RollingAccessBadgeBatchStatus.Resolved:
                return next == RollingAccessBadgeBatchStatus.Reopened || next == RollingAccessBadgeBatchStatus.Archived;
            case RollingAccessBadgeBatchStatus.Reopened:
                switch (next)
                {
                    case RollingAccessBadgeBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccessBadgeBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling access badge batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt + Hours, 2);
    }

    public RollingAccessBadgeBatch Copy()
    {
        return new RollingAccessBadgeBatch(
            Code,
            Title,
            Label,
            Quantity,
            Hours,
            EndsAt,
            CostCenter,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
