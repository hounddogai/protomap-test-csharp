namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default exception digest is in its life cycle.</summary>
public enum DefaultExceptionDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One default exception digest, with what the planners need to act on it.</summary>
public sealed class DefaultExceptionDigest(string code, string label, string segment, int updatedAt, double amount, int quantity, string locale, DefaultExceptionDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public int Quantity { get; set; } = quantity;
    public string Locale { get; set; } = locale;
    public DefaultExceptionDigestStatus Status { get; set; } = status;

    /// <summary>Starts a default exception digest in status open.</summary>
    public static DefaultExceptionDigest NewDefaultExceptionDigest(string code, string label)
    {
        return new DefaultExceptionDigest(code, label, "", 0, 0.0, 0, "", DefaultExceptionDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultExceptionDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultExceptionDigestStatus next)
    {
        switch (Status)
        {
            case DefaultExceptionDigestStatus.Open:
                return next == DefaultExceptionDigestStatus.InReview;
            case DefaultExceptionDigestStatus.InReview:
                switch (next)
                {
                    case DefaultExceptionDigestStatus.Archived:
                        return true;
                }
                return false;
            case DefaultExceptionDigestStatus.Resolved:
                switch (next)
                {
                    case DefaultExceptionDigestStatus.Reopened:
                    case DefaultExceptionDigestStatus.Archived:
                        return true;
                }
                return false;
            case DefaultExceptionDigestStatus.Reopened:
                return next == DefaultExceptionDigestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultExceptionDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default exception digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"default exception digest"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt - (double)Quantity, 2);
    }

    public DefaultExceptionDigest Copy()
    {
        return new DefaultExceptionDigest(Code, Label, Segment, UpdatedAt, Amount, Quantity, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
