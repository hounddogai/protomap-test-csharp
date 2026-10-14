namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered laptop notice is in its life cycle.</summary>
public enum TieredLaptopNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered laptop notice. Its code is unique within a tenant.</summary>
public sealed class TieredLaptopNotice(string code, string title, string contactEmail, double maximum, int revision, TieredLaptopNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Maximum { get; set; } = maximum;
    public int Revision { get; set; } = revision;
    public TieredLaptopNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a tiered laptop notice in status draft.</summary>
    public static TieredLaptopNotice NewTieredLaptopNotice(string code, string title)
    {
        return new TieredLaptopNotice(code, title, "", 0.0, 0, TieredLaptopNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLaptopNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLaptopNoticeStatus next)
    {
        switch (Status)
        {
            case TieredLaptopNoticeStatus.Draft:
                return next == TieredLaptopNoticeStatus.Pending || next == TieredLaptopNoticeStatus.Suspended;
            case TieredLaptopNoticeStatus.Pending:
                switch (next)
                {
                    case TieredLaptopNoticeStatus.Active:
                    case TieredLaptopNoticeStatus.Closed:
                        return true;
                }
                return false;
            case TieredLaptopNoticeStatus.Active:
                return next == TieredLaptopNoticeStatus.Suspended;
            case TieredLaptopNoticeStatus.Suspended:
                switch (next)
                {
                    case TieredLaptopNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLaptopNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered laptop notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Maximum * (double)Revision, 2);
    }

    public TieredLaptopNotice Copy()
    {
        return new TieredLaptopNotice(Code, Title, ContactEmail, Maximum, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
