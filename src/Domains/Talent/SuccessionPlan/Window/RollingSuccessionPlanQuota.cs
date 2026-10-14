namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling succession plan quota is in its life cycle.</summary>
public enum RollingSuccessionPlanQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling succession plan quota. Its code is unique within a tenant.</summary>
public sealed class RollingSuccessionPlanQuota(string code, string label, string source, int sequence, double ceiling, string location, int capacity, RollingSuccessionPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public int Capacity { get; set; } = capacity;
    public RollingSuccessionPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a rolling succession plan quota in status draft.</summary>
    public static RollingSuccessionPlanQuota NewRollingSuccessionPlanQuota(string code, string label)
    {
        return new RollingSuccessionPlanQuota(
            code,
            label,
            "",
            0,
            0.0,
            "",
            0,
            RollingSuccessionPlanQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSuccessionPlanQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSuccessionPlanQuotaStatus next)
    {
        switch (Status)
        {
            case RollingSuccessionPlanQuotaStatus.Draft:
                switch (next)
                {
                    case RollingSuccessionPlanQuotaStatus.Active:
                    case RollingSuccessionPlanQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case RollingSuccessionPlanQuotaStatus.Pending:
                switch (next)
                {
                    case RollingSuccessionPlanQuotaStatus.Active:
                    case RollingSuccessionPlanQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case RollingSuccessionPlanQuotaStatus.Active:
                return next == RollingSuccessionPlanQuotaStatus.Suspended || next == RollingSuccessionPlanQuotaStatus.Closed;
            case RollingSuccessionPlanQuotaStatus.Suspended:
                return next == RollingSuccessionPlanQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingSuccessionPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling succession plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling succession plan quota"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ceiling * (double)Capacity, 2);
    }

    public RollingSuccessionPlanQuota Copy()
    {
        return new RollingSuccessionPlanQuota(Code, Label, Source, Sequence, Ceiling, Location, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
