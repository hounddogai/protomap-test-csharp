namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty notice is in its life cycle.</summary>
public enum WarrantyNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A warranty notice as the warranty team keeps it.</summary>
public sealed class WarrantyNotice(string code, string title, string phone, double budget, int sequence, double rating, string jobCode, double cost, string region, double discount, WarrantyNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double Budget { get; set; } = budget;
    public int Sequence { get; set; } = sequence;
    public double Rating { get; set; } = rating;
    public string JobCode { get; set; } = jobCode;
    public double Cost { get; set; } = cost;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public WarrantyNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a warranty notice in status new.</summary>
    public static WarrantyNotice NewWarrantyNotice(string code, string title)
    {
        return new WarrantyNotice(code, title, "", 0.0, 0, 0.0, "", 0.0, "", 0.0, WarrantyNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyNoticeStatus next)
    {
        switch (Status)
        {
            case WarrantyNoticeStatus.New:
                return next == WarrantyNoticeStatus.Queued;
            case WarrantyNoticeStatus.Queued:
                switch (next)
                {
                    case WarrantyNoticeStatus.Running:
                        return true;
                }
                return false;
            case WarrantyNoticeStatus.Running:
                switch (next)
                {
                    case WarrantyNoticeStatus.Done:
                    case WarrantyNoticeStatus.Failed:
                        return true;
                }
                return false;
            case WarrantyNoticeStatus.Done:
                switch (next)
                {
                    case WarrantyNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"warranty notice"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating - Discount, 2);
    }

    public WarrantyNotice Copy()
    {
        return new WarrantyNotice(
            Code,
            Title,
            Phone,
            Budget,
            Sequence,
            Rating,
            JobCode,
            Cost,
            Region,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
