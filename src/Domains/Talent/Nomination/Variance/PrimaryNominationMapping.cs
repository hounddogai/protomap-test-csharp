namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary nomination mapping is in its life cycle.</summary>
public enum PrimaryNominationMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a primary nomination mapping and its current status.</summary>
public sealed class PrimaryNominationMapping(string code, string name, int graceDays, double amount, int tenureMonths, string costCenter, List<string> tags, string reviewer, double score, PrimaryNominationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public PrimaryNominationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary nomination mapping in status new.</summary>
    public static PrimaryNominationMapping NewPrimaryNominationMapping(string code, string name)
    {
        return new PrimaryNominationMapping(
            code,
            name,
            0,
            0.0,
            0,
            "",
            new List<string>(),
            "",
            0.0,
            PrimaryNominationMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryNominationMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryNominationMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryNominationMappingStatus.New:
                return next == PrimaryNominationMappingStatus.Failed;
            case PrimaryNominationMappingStatus.Queued:
                switch (next)
                {
                    case PrimaryNominationMappingStatus.Running:
                        return true;
                }
                return false;
            case PrimaryNominationMappingStatus.Running:
                switch (next)
                {
                    case PrimaryNominationMappingStatus.Done:
                    case PrimaryNominationMappingStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryNominationMappingStatus.Done:
                switch (next)
                {
                    case PrimaryNominationMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryNominationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary nomination mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)GraceDays * Score, 2);
    }

    public PrimaryNominationMapping Copy()
    {
        return new PrimaryNominationMapping(
            Code,
            Name,
            GraceDays,
            Amount,
            TenureMonths,
            CostCenter,
            Tags,
            Reviewer,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
