namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay audit is in its life cycle.</summary>
public enum RetroPayAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a retro pay audit and its current status.</summary>
public sealed class RetroPayAudit(string code, string name, string source, int startsAt, double margin, RetroPayAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public int StartsAt { get; set; } = startsAt;
    public double Margin { get; set; } = margin;
    public RetroPayAuditStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay audit in status draft.</summary>
    public static RetroPayAudit NewRetroPayAudit(string code, string name)
    {
        return new RetroPayAudit(code, name, "", 0, 0.0, RetroPayAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayAuditStatus next)
    {
        switch (Status)
        {
            case RetroPayAuditStatus.Draft:
                switch (next)
                {
                    case RetroPayAuditStatus.Active:
                        return true;
                }
                return false;
            case RetroPayAuditStatus.Pending:
                switch (next)
                {
                    case RetroPayAuditStatus.Closed:
                        return true;
                }
                return false;
            case RetroPayAuditStatus.Active:
                return next == RetroPayAuditStatus.Suspended;
            case RetroPayAuditStatus.Suspended:
                return next == RetroPayAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RetroPayAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin - (double)StartsAt, 2);
    }

    public RetroPayAudit Copy()
    {
        return new RetroPayAudit(Code, Name, Source, StartsAt, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
