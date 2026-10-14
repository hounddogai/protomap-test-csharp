namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment threshold is in its life cycle.</summary>
public enum AdjustmentThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adjustment threshold and its current status.</summary>
public sealed class AdjustmentThreshold(string code, string label, string source, string jobCode, string skill, bool active, string badge, int limit, AdjustmentThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string JobCode { get; set; } = jobCode;
    public string Skill { get; set; } = skill;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public AdjustmentThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment threshold in status requested.</summary>
    public static AdjustmentThreshold NewAdjustmentThreshold(string code, string label)
    {
        return new AdjustmentThreshold(code, label, "", "", "", false, "", 0, AdjustmentThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentThresholdStatus next)
    {
        switch (Status)
        {
            case AdjustmentThresholdStatus.Requested:
                return next == AdjustmentThresholdStatus.Granted || next == AdjustmentThresholdStatus.Expired;
            case AdjustmentThresholdStatus.Granted:
                switch (next)
                {
                    case AdjustmentThresholdStatus.Revoked:
                    case AdjustmentThresholdStatus.Expired:
                        return true;
                }
                return false;
            case AdjustmentThresholdStatus.Revoked:
                return next == AdjustmentThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdjustmentThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment threshold"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public AdjustmentThreshold Copy()
    {
        return new AdjustmentThreshold(Code, Label, Source, JobCode, Skill, Active, Badge, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
