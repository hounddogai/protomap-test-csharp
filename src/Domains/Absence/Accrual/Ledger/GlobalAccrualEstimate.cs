namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global accrual estimate is in its life cycle.</summary>
public enum GlobalAccrualEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global accrual estimate. Its code is unique within a tenant.</summary>
public sealed class GlobalAccrualEstimate(string code, string name, double minimum, string skill, double maximum, double rate, bool pinned, int revision, string badge, GlobalAccrualEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public bool Pinned { get; set; } = pinned;
    public int Revision { get; set; } = revision;
    public string Badge { get; set; } = badge;
    public GlobalAccrualEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a global accrual estimate in status open.</summary>
    public static GlobalAccrualEstimate NewGlobalAccrualEstimate(string code, string name)
    {
        return new GlobalAccrualEstimate(
            code,
            name,
            0.0,
            "",
            0.0,
            0.0,
            false,
            0,
            "",
            GlobalAccrualEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAccrualEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAccrualEstimateStatus next)
    {
        switch (Status)
        {
            case GlobalAccrualEstimateStatus.Open:
                return next == GlobalAccrualEstimateStatus.Archived;
            case GlobalAccrualEstimateStatus.InReview:
                switch (next)
                {
                    case GlobalAccrualEstimateStatus.Resolved:
                    case GlobalAccrualEstimateStatus.Archived:
                        return true;
                }
                return false;
            case GlobalAccrualEstimateStatus.Resolved:
                return next == GlobalAccrualEstimateStatus.Reopened;
            case GlobalAccrualEstimateStatus.Reopened:
                return next == GlobalAccrualEstimateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalAccrualEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global accrual estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"global accrual estimate"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum * Rate, 2);
    }

    public GlobalAccrualEstimate Copy()
    {
        return new GlobalAccrualEstimate(Code, Name, Minimum, Skill, Maximum, Rate, Pinned, Revision, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
