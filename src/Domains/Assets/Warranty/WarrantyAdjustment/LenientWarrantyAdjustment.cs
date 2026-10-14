namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient warranty adjustment is in its life cycle.</summary>
public enum LenientWarrantyAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a lenient warranty adjustment and its current status.</summary>
public sealed class LenientWarrantyAdjustment(string code, string title, double amount, string timezone, int windowDays, int priority, double rate, string label, LenientWarrantyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public int WindowDays { get; set; } = windowDays;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public string Label { get; set; } = label;
    public LenientWarrantyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient warranty adjustment in status created.</summary>
    public static LenientWarrantyAdjustment NewLenientWarrantyAdjustment(string code, string title)
    {
        return new LenientWarrantyAdjustment(
            code,
            title,
            0.0,
            "",
            0,
            0,
            0.0,
            "",
            LenientWarrantyAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientWarrantyAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientWarrantyAdjustmentStatus next)
    {
        switch (Status)
        {
            case LenientWarrantyAdjustmentStatus.Created:
                switch (next)
                {
                    case LenientWarrantyAdjustmentStatus.Validated:
                    case LenientWarrantyAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case LenientWarrantyAdjustmentStatus.Validated:
                switch (next)
                {
                    case LenientWarrantyAdjustmentStatus.Published:
                    case LenientWarrantyAdjustmentStatus.Retired:
                        return true;
                }
                return false;
            case LenientWarrantyAdjustmentStatus.Published:
                switch (next)
                {
                    case LenientWarrantyAdjustmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientWarrantyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient warranty adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient warranty adjustment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Priority - Rate, 2);
    }

    public LenientWarrantyAdjustment Copy()
    {
        return new LenientWarrantyAdjustment(
            Code,
            Title,
            Amount,
            Timezone,
            WindowDays,
            Priority,
            Rate,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
