namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility tier is in its life cycle.</summary>
public enum MobilityTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a mobility tier and its current status.</summary>
public sealed class MobilityTier(string code, string label, int capacity, string unit, string salaryBand, double cost, MobilityTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public double Cost { get; set; } = cost;
    public MobilityTierStatus Status { get; set; } = status;

    /// <summary>Starts a mobility tier in status open.</summary>
    public static MobilityTier NewMobilityTier(string code, string label)
    {
        return new MobilityTier(code, label, 0, "", "", 0.0, MobilityTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityTierStatus next)
    {
        switch (Status)
        {
            case MobilityTierStatus.Open:
                return next == MobilityTierStatus.InReview || next == MobilityTierStatus.Reopened;
            case MobilityTierStatus.InReview:
                switch (next)
                {
                    case MobilityTierStatus.Resolved:
                    case MobilityTierStatus.Reopened:
                        return true;
                }
                return false;
            case MobilityTierStatus.Resolved:
                return next == MobilityTierStatus.Archived;
            case MobilityTierStatus.Reopened:
                switch (next)
                {
                    case MobilityTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"mobility tier"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost + (double)Capacity, 2);
    }

    public MobilityTier Copy()
    {
        return new MobilityTier(Code, Label, Capacity, Unit, SalaryBand, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
