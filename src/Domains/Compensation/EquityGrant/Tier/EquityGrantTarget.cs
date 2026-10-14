namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant target is in its life cycle.</summary>
public enum EquityGrantTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A equity grant target. Its code is unique within a tenant.</summary>
public sealed class EquityGrantTarget(string code, string label, int batchSize, string costCenter, string unit, string currency, EquityGrantTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public EquityGrantTargetStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant target in status open.</summary>
    public static EquityGrantTarget NewEquityGrantTarget(string code, string label)
    {
        return new EquityGrantTarget(code, label, 0, "", "", "", EquityGrantTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantTargetStatus next)
    {
        switch (Status)
        {
            case EquityGrantTargetStatus.Open:
                return next == EquityGrantTargetStatus.InReview || next == EquityGrantTargetStatus.Archived;
            case EquityGrantTargetStatus.InReview:
                switch (next)
                {
                    case EquityGrantTargetStatus.Resolved:
                    case EquityGrantTargetStatus.Reopened:
                        return true;
                }
                return false;
            case EquityGrantTargetStatus.Resolved:
                switch (next)
                {
                    case EquityGrantTargetStatus.Reopened:
                        return true;
                }
                return false;
            case EquityGrantTargetStatus.Reopened:
                return next == EquityGrantTargetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(EquityGrantTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public EquityGrantTarget Copy()
    {
        return new EquityGrantTarget(Code, Label, BatchSize, CostCenter, Unit, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
