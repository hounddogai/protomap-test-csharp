namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending span exception is in its life cycle.</summary>
public enum PendingSpanExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pending span exception, with what the planners need to act on it.</summary>
public sealed class PendingSpanException(string code, string name, int updatedAt, string contactEmail, double discount, int retries, string managerId, string checksum, PendingSpanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ContactEmail { get; set; } = contactEmail;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public string ManagerId { get; set; } = managerId;
    public string Checksum { get; set; } = checksum;
    public PendingSpanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pending span exception in status open.</summary>
    public static PendingSpanException NewPendingSpanException(string code, string name)
    {
        return new PendingSpanException(code, name, 0, "", 0.0, 0, "", "", PendingSpanExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSpanExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSpanExceptionStatus next)
    {
        switch (Status)
        {
            case PendingSpanExceptionStatus.Open:
                switch (next)
                {
                    case PendingSpanExceptionStatus.Reopened:
                        return true;
                }
                return false;
            case PendingSpanExceptionStatus.InReview:
                switch (next)
                {
                    case PendingSpanExceptionStatus.Archived:
                        return true;
                }
                return false;
            case PendingSpanExceptionStatus.Resolved:
                return next == PendingSpanExceptionStatus.Reopened || next == PendingSpanExceptionStatus.Archived;
            case PendingSpanExceptionStatus.Reopened:
                return next == PendingSpanExceptionStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingSpanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending span exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending span exception"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Discount - (double)UpdatedAt, 2);
    }

    public PendingSpanException Copy()
    {
        return new PendingSpanException(
            Code,
            Name,
            UpdatedAt,
            ContactEmail,
            Discount,
            Retries,
            ManagerId,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
