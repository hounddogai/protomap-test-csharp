namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit segment is in its life cycle.</summary>
public enum AuditSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A audit segment. Its code is unique within a tenant.</summary>
public sealed class AuditSegment(string code, string title, int quantity, string jobCode, int graceDays, string contactEmail, double margin, AuditSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public AuditSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a audit segment in status draft.</summary>
    public static AuditSegment NewAuditSegment(string code, string title)
    {
        return new AuditSegment(code, title, 0, "", 0, "", 0.0, AuditSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditSegmentStatus next)
    {
        switch (Status)
        {
            case AuditSegmentStatus.Draft:
                return next == AuditSegmentStatus.Pending;
            case AuditSegmentStatus.Pending:
                switch (next)
                {
                    case AuditSegmentStatus.Active:
                    case AuditSegmentStatus.Suspended:
                        return true;
                }
                return false;
            case AuditSegmentStatus.Active:
                return next == AuditSegmentStatus.Suspended;
            case AuditSegmentStatus.Suspended:
                switch (next)
                {
                    case AuditSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit segment"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Margin - (double)GraceDays, 2);
    }

    public AuditSegment Copy()
    {
        return new AuditSegment(Code, Title, Quantity, JobCode, GraceDays, ContactEmail, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
