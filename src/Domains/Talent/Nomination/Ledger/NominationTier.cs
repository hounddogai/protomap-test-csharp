namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination tier is in its life cycle.</summary>
public enum NominationTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A nomination tier as the nomination team keeps it.</summary>
public sealed class NominationTier(string code, string label, double cost, int capacity, string externalRef, bool approved, double score, string summary, NominationTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public string Summary { get; set; } = summary;
    public NominationTierStatus Status { get; set; } = status;

    /// <summary>Starts a nomination tier in status new.</summary>
    public static NominationTier NewNominationTier(string code, string label)
    {
        return new NominationTier(code, label, 0.0, 0, "", false, 0.0, "", NominationTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationTierStatus next)
    {
        switch (Status)
        {
            case NominationTierStatus.New:
                switch (next)
                {
                    case NominationTierStatus.Done:
                        return true;
                }
                return false;
            case NominationTierStatus.Queued:
                switch (next)
                {
                    case NominationTierStatus.Running:
                    case NominationTierStatus.Done:
                        return true;
                }
                return false;
            case NominationTierStatus.Running:
                return next == NominationTierStatus.Done || next == NominationTierStatus.Failed;
            case NominationTierStatus.Done:
                return next == NominationTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(NominationTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Score * Cost, 2);
    }

    public NominationTier Copy()
    {
        return new NominationTier(Code, Label, Cost, Capacity, ExternalRef, Approved, Score, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
