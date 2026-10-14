namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position quota is in its life cycle.</summary>
public enum PositionQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a position quota and its current status.</summary>
public sealed class PositionQuota(string code, string label, bool archived, string channel, string jobCode, double amount, PositionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public PositionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a position quota in status idle.</summary>
    public static PositionQuota NewPositionQuota(string code, string label)
    {
        return new PositionQuota(code, label, false, "", "", 0.0, PositionQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionQuotaStatus next)
    {
        switch (Status)
        {
            case PositionQuotaStatus.Idle:
                switch (next)
                {
                    case PositionQuotaStatus.Ready:
                        return true;
                }
                return false;
            case PositionQuotaStatus.Warming:
                switch (next)
                {
                    case PositionQuotaStatus.Ready:
                    case PositionQuotaStatus.Draining:
                        return true;
                }
                return false;
            case PositionQuotaStatus.Ready:
                switch (next)
                {
                    case PositionQuotaStatus.Draining:
                    case PositionQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case PositionQuotaStatus.Draining:
                return next == PositionQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PositionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PositionQuota Copy()
    {
        return new PositionQuota(Code, Label, Archived, Channel, JobCode, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
