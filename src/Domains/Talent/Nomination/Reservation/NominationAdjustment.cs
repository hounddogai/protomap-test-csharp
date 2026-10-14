namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination adjustment is in its life cycle.</summary>
public enum NominationAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a nomination adjustment and its current status.</summary>
public sealed class NominationAdjustment(string code, string title, string region, string phone, double discount, string name, double weight, string segment, string category, NominationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Phone { get; set; } = phone;
    public double Discount { get; set; } = discount;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public string Category { get; set; } = category;
    public NominationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a nomination adjustment in status idle.</summary>
    public static NominationAdjustment NewNominationAdjustment(string code, string title)
    {
        return new NominationAdjustment(code, title, "", "", 0.0, "", 0.0, "", "", NominationAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationAdjustmentStatus next)
    {
        switch (Status)
        {
            case NominationAdjustmentStatus.Idle:
                switch (next)
                {
                    case NominationAdjustmentStatus.Ready:
                    case NominationAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case NominationAdjustmentStatus.Warming:
                return next == NominationAdjustmentStatus.Draining || next == NominationAdjustmentStatus.Stopped;
            case NominationAdjustmentStatus.Ready:
                return next == NominationAdjustmentStatus.Draining;
            case NominationAdjustmentStatus.Draining:
                switch (next)
                {
                    case NominationAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight * Discount, 2);
    }

    public NominationAdjustment Copy()
    {
        return new NominationAdjustment(
            Code,
            Title,
            Region,
            Phone,
            Discount,
            Name,
            Weight,
            Segment,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
