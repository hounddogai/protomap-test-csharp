namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched rate preference is in its life cycle.</summary>
public enum BatchedRatePreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched rate preference as the rate team keeps it.</summary>
public sealed class BatchedRatePreference(string code, string title, double ratio, double fte, string reviewer, string segment, double budget, int capacity, double minimum, string badge, BatchedRatePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public string Reviewer { get; set; } = reviewer;
    public string Segment { get; set; } = segment;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public string Badge { get; set; } = badge;
    public BatchedRatePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a batched rate preference in status idle.</summary>
    public static BatchedRatePreference NewBatchedRatePreference(string code, string title)
    {
        return new BatchedRatePreference(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0,
            0.0,
            "",
            BatchedRatePreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRatePreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRatePreferenceStatus next)
    {
        switch (Status)
        {
            case BatchedRatePreferenceStatus.Idle:
                switch (next)
                {
                    case BatchedRatePreferenceStatus.Ready:
                        return true;
                }
                return false;
            case BatchedRatePreferenceStatus.Warming:
                return next == BatchedRatePreferenceStatus.Stopped;
            case BatchedRatePreferenceStatus.Ready:
                return next == BatchedRatePreferenceStatus.Draining || next == BatchedRatePreferenceStatus.Stopped;
            case BatchedRatePreferenceStatus.Draining:
                switch (next)
                {
                    case BatchedRatePreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRatePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched rate preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ratio - Budget, 2);
    }

    public BatchedRatePreference Copy()
    {
        return new BatchedRatePreference(
            Code,
            Title,
            Ratio,
            Fte,
            Reviewer,
            Segment,
            Budget,
            Capacity,
            Minimum,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
