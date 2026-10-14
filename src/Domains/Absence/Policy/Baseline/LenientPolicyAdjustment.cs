namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy adjustment is in its life cycle.</summary>
public enum LenientPolicyAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One lenient policy adjustment, with what the planners need to act on it.</summary>
public sealed class LenientPolicyAdjustment(string code, string name, string checksum, bool active, string currency, string locale, double discount, string managerId, string title, double threshold, LenientPolicyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public LenientPolicyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy adjustment in status idle.</summary>
    public static LenientPolicyAdjustment NewLenientPolicyAdjustment(string code, string name)
    {
        return new LenientPolicyAdjustment(
            code,
            name,
            "",
            false,
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            LenientPolicyAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyAdjustmentStatus next)
    {
        switch (Status)
        {
            case LenientPolicyAdjustmentStatus.Idle:
                switch (next)
                {
                    case LenientPolicyAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case LenientPolicyAdjustmentStatus.Warming:
                return next == LenientPolicyAdjustmentStatus.Ready || next == LenientPolicyAdjustmentStatus.Stopped;
            case LenientPolicyAdjustmentStatus.Ready:
                switch (next)
                {
                    case LenientPolicyAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case LenientPolicyAdjustmentStatus.Draining:
                return next == LenientPolicyAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientPolicyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient policy adjustment"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount - Threshold, 2);
    }

    public LenientPolicyAdjustment Copy()
    {
        return new LenientPolicyAdjustment(
            Code,
            Name,
            Checksum,
            Active,
            Currency,
            Locale,
            Discount,
            ManagerId,
            Title,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
