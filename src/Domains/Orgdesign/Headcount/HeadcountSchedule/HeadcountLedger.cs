namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount ledger is in its life cycle.</summary>
public enum HeadcountLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A headcount ledger as the headcount team keeps it.</summary>
public sealed class HeadcountLedger(string code, string title, double rating, string department, double weight, Dictionary<string, string> attributes, int durationMinutes, string reviewer, HeadcountLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public HeadcountLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a headcount ledger in status idle.</summary>
    public static HeadcountLedger NewHeadcountLedger(string code, string title)
    {
        return new HeadcountLedger(
            code,
            title,
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            HeadcountLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountLedgerStatus next)
    {
        switch (Status)
        {
            case HeadcountLedgerStatus.Idle:
                switch (next)
                {
                    case HeadcountLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case HeadcountLedgerStatus.Warming:
                return next == HeadcountLedgerStatus.Stopped;
            case HeadcountLedgerStatus.Ready:
                switch (next)
                {
                    case HeadcountLedgerStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountLedgerStatus.Draining:
                return next == HeadcountLedgerStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HeadcountLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedExposure()
    {
        return Math.Round(Weight * Rating, 2);
    }

    public HeadcountLedger Copy()
    {
        return new HeadcountLedger(
            Code,
            Title,
            Rating,
            Department,
            Weight,
            Attributes,
            DurationMinutes,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
