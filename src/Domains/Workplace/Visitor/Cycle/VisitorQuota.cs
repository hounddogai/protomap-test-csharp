namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor quota is in its life cycle.</summary>
public enum VisitorQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a visitor quota and its current status.</summary>
public sealed class VisitorQuota(string code, string label, string unit, double ceiling, int expiresAt, string source, VisitorQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Source { get; set; } = source;
    public VisitorQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a visitor quota in status idle.</summary>
    public static VisitorQuota NewVisitorQuota(string code, string label)
    {
        return new VisitorQuota(code, label, "", 0.0, 0, "", VisitorQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorQuotaStatus next)
    {
        switch (Status)
        {
            case VisitorQuotaStatus.Idle:
                switch (next)
                {
                    case VisitorQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorQuotaStatus.Warming:
                return next == VisitorQuotaStatus.Stopped;
            case VisitorQuotaStatus.Ready:
                switch (next)
                {
                    case VisitorQuotaStatus.Draining:
                    case VisitorQuotaStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorQuotaStatus.Draining:
                switch (next)
                {
                    case VisitorQuotaStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt + Ceiling, 2);
    }

    public VisitorQuota Copy()
    {
        return new VisitorQuota(Code, Label, Unit, Ceiling, ExpiresAt, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
