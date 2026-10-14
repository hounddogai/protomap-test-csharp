namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending bonus pool policy is in its life cycle.</summary>
public enum PendingBonusPoolPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending bonus pool policy. Its code is unique within a tenant.</summary>
public sealed class PendingBonusPoolPolicy(string code, string title, string unit, double ratio, string salaryBand, string category, bool active, PendingBonusPoolPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public string SalaryBand { get; set; } = salaryBand;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public PendingBonusPoolPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pending bonus pool policy in status new.</summary>
    public static PendingBonusPoolPolicy NewPendingBonusPoolPolicy(string code, string title)
    {
        return new PendingBonusPoolPolicy(code, title, "", 0.0, "", "", false, PendingBonusPoolPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingBonusPoolPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingBonusPoolPolicyStatus next)
    {
        switch (Status)
        {
            case PendingBonusPoolPolicyStatus.New:
                switch (next)
                {
                    case PendingBonusPoolPolicyStatus.Queued:
                    case PendingBonusPoolPolicyStatus.Running:
                        return true;
                }
                return false;
            case PendingBonusPoolPolicyStatus.Queued:
                switch (next)
                {
                    case PendingBonusPoolPolicyStatus.Running:
                        return true;
                }
                return false;
            case PendingBonusPoolPolicyStatus.Running:
                switch (next)
                {
                    case PendingBonusPoolPolicyStatus.Done:
                        return true;
                }
                return false;
            case PendingBonusPoolPolicyStatus.Done:
                return next == PendingBonusPoolPolicyStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingBonusPoolPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending bonus pool policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending bonus pool policy"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public PendingBonusPoolPolicy Copy()
    {
        return new PendingBonusPoolPolicy(Code, Title, Unit, Ratio, SalaryBand, Category, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
