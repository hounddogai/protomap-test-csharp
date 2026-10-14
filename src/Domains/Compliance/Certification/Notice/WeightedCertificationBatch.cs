namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted certification batch is in its life cycle.</summary>
public enum WeightedCertificationBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted certification batch as the certification team keeps it.</summary>
public sealed class WeightedCertificationBatch(string code, string title, int limit, int durationMinutes, string employeeId, string phone, double ratio, double budget, string country, WeightedCertificationBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public double Ratio { get; set; } = ratio;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public WeightedCertificationBatchStatus Status { get; set; } = status;

    /// <summary>Starts a weighted certification batch in status idle.</summary>
    public static WeightedCertificationBatch NewWeightedCertificationBatch(string code, string title)
    {
        return new WeightedCertificationBatch(
            code,
            title,
            0,
            0,
            "",
            "",
            0.0,
            0.0,
            "",
            WeightedCertificationBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCertificationBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCertificationBatchStatus next)
    {
        switch (Status)
        {
            case WeightedCertificationBatchStatus.Idle:
                return next == WeightedCertificationBatchStatus.Stopped;
            case WeightedCertificationBatchStatus.Warming:
                switch (next)
                {
                    case WeightedCertificationBatchStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedCertificationBatchStatus.Ready:
                switch (next)
                {
                    case WeightedCertificationBatchStatus.Draining:
                    case WeightedCertificationBatchStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedCertificationBatchStatus.Draining:
                return next == WeightedCertificationBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WeightedCertificationBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted certification batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted certification batch"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Budget * (double)DurationMinutes, 2);
    }

    public WeightedCertificationBatch Copy()
    {
        return new WeightedCertificationBatch(
            Code,
            Title,
            Limit,
            DurationMinutes,
            EmployeeId,
            Phone,
            Ratio,
            Budget,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
