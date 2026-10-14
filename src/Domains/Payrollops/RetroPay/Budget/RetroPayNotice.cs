namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay notice is in its life cycle.</summary>
public enum RetroPayNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a retro pay notice and its current status.</summary>
public sealed class RetroPayNotice(string code, string label, string phone, string skill, string name, double amount, double rate, string badge, int quantity, string jobCode, bool pinned, RetroPayNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Skill { get; set; } = skill;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public string JobCode { get; set; } = jobCode;
    public bool Pinned { get; set; } = pinned;
    public RetroPayNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay notice in status created.</summary>
    public static RetroPayNotice NewRetroPayNotice(string code, string label)
    {
        return new RetroPayNotice(code, label, "", "", "", 0.0, 0.0, "", 0, "", false, RetroPayNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayNoticeStatus next)
    {
        switch (Status)
        {
            case RetroPayNoticeStatus.Created:
                return next == RetroPayNoticeStatus.Validated;
            case RetroPayNoticeStatus.Validated:
                return next == RetroPayNoticeStatus.Retired;
            case RetroPayNoticeStatus.Published:
                return next == RetroPayNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay notice"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Quantity - Rate, 2);
    }

    public RetroPayNotice Copy()
    {
        return new RetroPayNotice(
            Code,
            Label,
            Phone,
            Skill,
            Name,
            Amount,
            Rate,
            Badge,
            Quantity,
            JobCode,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
