namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted headcount notice is in its life cycle.</summary>
public enum WeightedHeadcountNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a weighted headcount notice and its current status.</summary>
public sealed class WeightedHeadcountNotice(string code, string label, string title, string jobCode, bool approved, string currency, string phone, int quantity, WeightedHeadcountNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public WeightedHeadcountNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted headcount notice in status idle.</summary>
    public static WeightedHeadcountNotice NewWeightedHeadcountNotice(string code, string label)
    {
        return new WeightedHeadcountNotice(
            code,
            label,
            "",
            "",
            false,
            "",
            "",
            0,
            WeightedHeadcountNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedHeadcountNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedHeadcountNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedHeadcountNoticeStatus.Idle:
                switch (next)
                {
                    case WeightedHeadcountNoticeStatus.Warming:
                        return true;
                }
                return false;
            case WeightedHeadcountNoticeStatus.Warming:
                return next == WeightedHeadcountNoticeStatus.Ready || next == WeightedHeadcountNoticeStatus.Stopped;
            case WeightedHeadcountNoticeStatus.Ready:
                return next == WeightedHeadcountNoticeStatus.Draining;
            case WeightedHeadcountNoticeStatus.Draining:
                switch (next)
                {
                    case WeightedHeadcountNoticeStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedHeadcountNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted headcount notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public WeightedHeadcountNotice Copy()
    {
        return new WeightedHeadcountNotice(Code, Label, Title, JobCode, Approved, Currency, Phone, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
