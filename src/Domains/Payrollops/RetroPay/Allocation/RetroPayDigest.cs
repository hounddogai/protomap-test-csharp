namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay digest is in its life cycle.</summary>
public enum RetroPayDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One retro pay digest, with what the planners need to act on it.</summary>
public sealed class RetroPayDigest(string code, string title, string category, string salaryBand, string checksum, bool visible, int quantity, RetroPayDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public int Quantity { get; set; } = quantity;
    public RetroPayDigestStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay digest in status draft.</summary>
    public static RetroPayDigest NewRetroPayDigest(string code, string title)
    {
        return new RetroPayDigest(code, title, "", "", "", false, 0, RetroPayDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayDigestStatus next)
    {
        switch (Status)
        {
            case RetroPayDigestStatus.Draft:
                return next == RetroPayDigestStatus.Suspended;
            case RetroPayDigestStatus.Pending:
                switch (next)
                {
                    case RetroPayDigestStatus.Active:
                    case RetroPayDigestStatus.Suspended:
                        return true;
                }
                return false;
            case RetroPayDigestStatus.Active:
                return next == RetroPayDigestStatus.Suspended || next == RetroPayDigestStatus.Closed;
            case RetroPayDigestStatus.Suspended:
                return next == RetroPayDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RetroPayDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay digest"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public RetroPayDigest Copy()
    {
        return new RetroPayDigest(Code, Title, Category, SalaryBand, Checksum, Visible, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
