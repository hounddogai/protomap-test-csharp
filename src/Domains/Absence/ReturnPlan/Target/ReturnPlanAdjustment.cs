namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan adjustment is in its life cycle.</summary>
public enum ReturnPlanAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A return plan adjustment as the return plan team keeps it.</summary>
public sealed class ReturnPlanAdjustment(string code, string name, int windowDays, string currency, string region, int quantity, int sequence, string phone, int updatedAt, string externalRef, ReturnPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Currency { get; set; } = currency;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public string Phone { get; set; } = phone;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public ReturnPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a return plan adjustment in status open.</summary>
    public static ReturnPlanAdjustment NewReturnPlanAdjustment(string code, string name)
    {
        return new ReturnPlanAdjustment(code, name, 0, "", "", 0, 0, "", 0, "", ReturnPlanAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case ReturnPlanAdjustmentStatus.Open:
                switch (next)
                {
                    case ReturnPlanAdjustmentStatus.Resolved:
                    case ReturnPlanAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanAdjustmentStatus.InReview:
                switch (next)
                {
                    case ReturnPlanAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case ReturnPlanAdjustmentStatus.Resolved:
                return next == ReturnPlanAdjustmentStatus.Reopened || next == ReturnPlanAdjustmentStatus.Archived;
            case ReturnPlanAdjustmentStatus.Reopened:
                return next == ReturnPlanAdjustmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ReturnPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt - (double)Quantity, 2);
    }

    public ReturnPlanAdjustment Copy()
    {
        return new ReturnPlanAdjustment(
            Code,
            Name,
            WindowDays,
            Currency,
            Region,
            Quantity,
            Sequence,
            Phone,
            UpdatedAt,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
