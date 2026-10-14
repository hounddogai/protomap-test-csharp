namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount notice is in its life cycle.</summary>
public enum HeadcountNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a headcount notice and its current status.</summary>
public sealed class HeadcountNotice(string code, string label, string summary, int endsAt, int revision, double discount, double ratio, int limit, string jobCode, string category, HeadcountNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public double Discount { get; set; } = discount;
    public double Ratio { get; set; } = ratio;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public string Category { get; set; } = category;
    public HeadcountNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a headcount notice in status proposed.</summary>
    public static HeadcountNotice NewHeadcountNotice(string code, string label)
    {
        return new HeadcountNotice(code, label, "", 0, 0, 0.0, 0.0, 0, "", "", HeadcountNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountNoticeStatus next)
    {
        switch (Status)
        {
            case HeadcountNoticeStatus.Proposed:
                switch (next)
                {
                    case HeadcountNoticeStatus.Rejected:
                    case HeadcountNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountNoticeStatus.Approved:
                switch (next)
                {
                    case HeadcountNoticeStatus.Rejected:
                    case HeadcountNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountNoticeStatus.Rejected:
                switch (next)
                {
                    case HeadcountNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount + (double)Revision, 2);
    }

    public HeadcountNotice Copy()
    {
        return new HeadcountNotice(
            Code,
            Label,
            Summary,
            EndsAt,
            Revision,
            Discount,
            Ratio,
            Limit,
            JobCode,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
