namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted access badge ledger is in its life cycle.</summary>
public enum WeightedAccessBadgeLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted access badge ledger. Its code is unique within a tenant.</summary>
public sealed class WeightedAccessBadgeLedger(string code, string name, string summary, double maximum, int graceDays, string timezone, int retries, string location, string title, int createdAt, WeightedAccessBadgeLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public string Timezone { get; set; } = timezone;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public WeightedAccessBadgeLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a weighted access badge ledger in status planned.</summary>
    public static WeightedAccessBadgeLedger NewWeightedAccessBadgeLedger(string code, string name)
    {
        return new WeightedAccessBadgeLedger(
            code,
            name,
            "",
            0.0,
            0,
            "",
            0,
            "",
            "",
            0,
            WeightedAccessBadgeLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccessBadgeLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccessBadgeLedgerStatus next)
    {
        switch (Status)
        {
            case WeightedAccessBadgeLedgerStatus.Planned:
                switch (next)
                {
                    case WeightedAccessBadgeLedgerStatus.Scheduled:
                        return true;
                }
                return false;
            case WeightedAccessBadgeLedgerStatus.Scheduled:
                return next == WeightedAccessBadgeLedgerStatus.Cancelled;
            case WeightedAccessBadgeLedgerStatus.InProgress:
                switch (next)
                {
                    case WeightedAccessBadgeLedgerStatus.Completed:
                    case WeightedAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedAccessBadgeLedgerStatus.Completed:
                switch (next)
                {
                    case WeightedAccessBadgeLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAccessBadgeLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted access badge ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Retries + (double)GraceDays, 2);
    }

    public WeightedAccessBadgeLedger Copy()
    {
        return new WeightedAccessBadgeLedger(
            Code,
            Name,
            Summary,
            Maximum,
            GraceDays,
            Timezone,
            Retries,
            Location,
            Title,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
