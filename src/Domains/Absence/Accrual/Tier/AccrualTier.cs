namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual tier is in its life cycle.</summary>
public enum AccrualTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a accrual tier and its current status.</summary>
public sealed class AccrualTier(string code, string label, string description, int retries, string location, double threshold, int attempts, double margin, string unit, AccrualTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public double Margin { get; set; } = margin;
    public string Unit { get; set; } = unit;
    public AccrualTierStatus Status { get; set; } = status;

    /// <summary>Starts a accrual tier in status new.</summary>
    public static AccrualTier NewAccrualTier(string code, string label)
    {
        return new AccrualTier(code, label, "", 0, "", 0.0, 0, 0.0, "", AccrualTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualTierStatus next)
    {
        switch (Status)
        {
            case AccrualTierStatus.New:
                return next == AccrualTierStatus.Done || next == AccrualTierStatus.Failed;
            case AccrualTierStatus.Queued:
                switch (next)
                {
                    case AccrualTierStatus.Running:
                    case AccrualTierStatus.Done:
                        return true;
                }
                return false;
            case AccrualTierStatus.Running:
                return next == AccrualTierStatus.Done || next == AccrualTierStatus.Failed;
            case AccrualTierStatus.Done:
                switch (next)
                {
                    case AccrualTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Retries * (double)Attempts, 2);
    }

    public AccrualTier Copy()
    {
        return new AccrualTier(
            Code,
            Label,
            Description,
            Retries,
            Location,
            Threshold,
            Attempts,
            Margin,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
