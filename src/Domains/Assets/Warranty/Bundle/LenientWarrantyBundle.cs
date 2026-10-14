namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient warranty bundle is in its life cycle.</summary>
public enum LenientWarrantyBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One lenient warranty bundle, with what the planners need to act on it.</summary>
public sealed class LenientWarrantyBundle(string code, string label, string skill, int priority, int tenureMonths, LenientWarrantyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public int TenureMonths { get; set; } = tenureMonths;
    public LenientWarrantyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient warranty bundle in status draft.</summary>
    public static LenientWarrantyBundle NewLenientWarrantyBundle(string code, string label)
    {
        return new LenientWarrantyBundle(code, label, "", 0, 0, LenientWarrantyBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientWarrantyBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientWarrantyBundleStatus next)
    {
        switch (Status)
        {
            case LenientWarrantyBundleStatus.Draft:
                return next == LenientWarrantyBundleStatus.Pending || next == LenientWarrantyBundleStatus.Suspended;
            case LenientWarrantyBundleStatus.Pending:
                return next == LenientWarrantyBundleStatus.Suspended || next == LenientWarrantyBundleStatus.Closed;
            case LenientWarrantyBundleStatus.Active:
                switch (next)
                {
                    case LenientWarrantyBundleStatus.Closed:
                        return true;
                }
                return false;
            case LenientWarrantyBundleStatus.Suspended:
                return next == LenientWarrantyBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientWarrantyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient warranty bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority + (double)TenureMonths, 2);
    }

    public LenientWarrantyBundle Copy()
    {
        return new LenientWarrantyBundle(Code, Label, Skill, Priority, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
