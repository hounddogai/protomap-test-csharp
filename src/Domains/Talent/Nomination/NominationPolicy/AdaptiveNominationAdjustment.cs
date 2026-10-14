namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive nomination adjustment is in its life cycle.</summary>
public enum AdaptiveNominationAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a adaptive nomination adjustment and its current status.</summary>
public sealed class AdaptiveNominationAdjustment(string code, string label, double hours, string phone, string name, int endsAt, int tenureMonths, AdaptiveNominationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public AdaptiveNominationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive nomination adjustment in status proposed.</summary>
    public static AdaptiveNominationAdjustment NewAdaptiveNominationAdjustment(string code, string label)
    {
        return new AdaptiveNominationAdjustment(
            code,
            label,
            0.0,
            "",
            "",
            0,
            0,
            AdaptiveNominationAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveNominationAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveNominationAdjustmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveNominationAdjustmentStatus.Proposed:
                return next == AdaptiveNominationAdjustmentStatus.Approved || next == AdaptiveNominationAdjustmentStatus.Withdrawn;
            case AdaptiveNominationAdjustmentStatus.Approved:
                switch (next)
                {
                    case AdaptiveNominationAdjustmentStatus.Rejected:
                    case AdaptiveNominationAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdaptiveNominationAdjustmentStatus.Rejected:
                switch (next)
                {
                    case AdaptiveNominationAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveNominationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive nomination adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive nomination adjustment"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours - (double)EndsAt, 2);
    }

    public AdaptiveNominationAdjustment Copy()
    {
        return new AdaptiveNominationAdjustment(Code, Label, Hours, Phone, Name, EndsAt, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
