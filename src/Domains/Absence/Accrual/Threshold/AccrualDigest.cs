namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual digest is in its life cycle.</summary>
public enum AccrualDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One accrual digest, with what the planners need to act on it.</summary>
public sealed class AccrualDigest(string code, string label, bool active, int quantity, double floorValue, string source, string locale, double threshold, double rate, string title, AccrualDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public string Source { get; set; } = source;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public double Rate { get; set; } = rate;
    public string Title { get; set; } = title;
    public AccrualDigestStatus Status { get; set; } = status;

    /// <summary>Starts a accrual digest in status requested.</summary>
    public static AccrualDigest NewAccrualDigest(string code, string label)
    {
        return new AccrualDigest(code, label, false, 0, 0.0, "", "", 0.0, 0.0, "", AccrualDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualDigestStatus next)
    {
        switch (Status)
        {
            case AccrualDigestStatus.Requested:
                switch (next)
                {
                    case AccrualDigestStatus.Granted:
                    case AccrualDigestStatus.Revoked:
                        return true;
                }
                return false;
            case AccrualDigestStatus.Granted:
                return next == AccrualDigestStatus.Revoked || next == AccrualDigestStatus.Expired;
            case AccrualDigestStatus.Revoked:
                switch (next)
                {
                    case AccrualDigestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"accrual digest"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Threshold * Rate, 2);
    }

    public AccrualDigest Copy()
    {
        return new AccrualDigest(
            Code,
            Label,
            Active,
            Quantity,
            FloorValue,
            Source,
            Locale,
            Threshold,
            Rate,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
