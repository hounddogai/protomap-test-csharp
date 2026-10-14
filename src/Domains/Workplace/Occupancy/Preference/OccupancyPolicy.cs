namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy policy is in its life cycle.</summary>
public enum OccupancyPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A occupancy policy as the occupancy team keeps it.</summary>
public sealed class OccupancyPolicy(string code, string title, int leaveDays, int batchSize, string timezone, string label, double score, int graceDays, int limit, string salaryBand, int sequence, OccupancyPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public string Timezone { get; set; } = timezone;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public OccupancyPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy policy in status open.</summary>
    public static OccupancyPolicy NewOccupancyPolicy(string code, string title)
    {
        return new OccupancyPolicy(code, title, 0, 0, "", "", 0.0, 0, 0, "", 0, OccupancyPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyPolicyStatus next)
    {
        switch (Status)
        {
            case OccupancyPolicyStatus.Open:
                switch (next)
                {
                    case OccupancyPolicyStatus.Resolved:
                        return true;
                }
                return false;
            case OccupancyPolicyStatus.InReview:
                switch (next)
                {
                    case OccupancyPolicyStatus.Reopened:
                    case OccupancyPolicyStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyPolicyStatus.Resolved:
                switch (next)
                {
                    case OccupancyPolicyStatus.Reopened:
                    case OccupancyPolicyStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyPolicyStatus.Reopened:
                switch (next)
                {
                    case OccupancyPolicyStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Limit - (double)GraceDays, 2);
    }

    public OccupancyPolicy Copy()
    {
        return new OccupancyPolicy(
            Code,
            Title,
            LeaveDays,
            BatchSize,
            Timezone,
            Label,
            Score,
            GraceDays,
            Limit,
            SalaryBand,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
