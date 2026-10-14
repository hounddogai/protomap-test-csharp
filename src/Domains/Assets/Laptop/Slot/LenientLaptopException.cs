namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient laptop exception is in its life cycle.</summary>
public enum LenientLaptopExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient laptop exception. Its code is unique within a tenant.</summary>
public sealed class LenientLaptopException(string code, string name, string title, double cost, string source, string currency, string segment, string grade, int createdAt, LenientLaptopExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public string Segment { get; set; } = segment;
    public string Grade { get; set; } = grade;
    public int CreatedAt { get; set; } = createdAt;
    public LenientLaptopExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient laptop exception in status requested.</summary>
    public static LenientLaptopException NewLenientLaptopException(string code, string name)
    {
        return new LenientLaptopException(
            code,
            name,
            "",
            0.0,
            "",
            "",
            "",
            "",
            0,
            LenientLaptopExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLaptopExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLaptopExceptionStatus next)
    {
        switch (Status)
        {
            case LenientLaptopExceptionStatus.Requested:
                switch (next)
                {
                    case LenientLaptopExceptionStatus.Revoked:
                        return true;
                }
                return false;
            case LenientLaptopExceptionStatus.Granted:
                return next == LenientLaptopExceptionStatus.Revoked || next == LenientLaptopExceptionStatus.Expired;
            case LenientLaptopExceptionStatus.Revoked:
                switch (next)
                {
                    case LenientLaptopExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLaptopExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient laptop exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)CreatedAt + Cost, 2);
    }

    public LenientLaptopException Copy()
    {
        return new LenientLaptopException(
            Code,
            Name,
            Title,
            Cost,
            Source,
            Currency,
            Segment,
            Grade,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
