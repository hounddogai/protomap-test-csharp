namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict reorg notice is in its life cycle.</summary>
public enum StrictReorgNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict reorg notice. Its code is unique within a tenant.</summary>
public sealed class StrictReorgNotice(string code, string label, string costCenter, string checksum, string source, int tenureMonths, StrictReorgNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public int TenureMonths { get; set; } = tenureMonths;
    public StrictReorgNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a strict reorg notice in status created.</summary>
    public static StrictReorgNotice NewStrictReorgNotice(string code, string label)
    {
        return new StrictReorgNotice(code, label, "", "", "", 0, StrictReorgNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReorgNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReorgNoticeStatus next)
    {
        switch (Status)
        {
            case StrictReorgNoticeStatus.Created:
                switch (next)
                {
                    case StrictReorgNoticeStatus.Validated:
                    case StrictReorgNoticeStatus.Published:
                        return true;
                }
                return false;
            case StrictReorgNoticeStatus.Validated:
                return next == StrictReorgNoticeStatus.Retired;
            case StrictReorgNoticeStatus.Published:
                return next == StrictReorgNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StrictReorgNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict reorg notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict reorg notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public StrictReorgNotice Copy()
    {
        return new StrictReorgNotice(Code, Label, CostCenter, Checksum, Source, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
