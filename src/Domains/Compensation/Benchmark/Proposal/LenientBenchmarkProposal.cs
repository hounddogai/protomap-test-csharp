namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient benchmark proposal is in its life cycle.</summary>
public enum LenientBenchmarkProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient benchmark proposal, with what the planners need to act on it.</summary>
public sealed class LenientBenchmarkProposal(string code, string title, string channel, bool active, int graceDays, double budget, int endsAt, bool approved, List<string> tags, LenientBenchmarkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public LenientBenchmarkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a lenient benchmark proposal in status planned.</summary>
    public static LenientBenchmarkProposal NewLenientBenchmarkProposal(string code, string title)
    {
        return new LenientBenchmarkProposal(
            code,
            title,
            "",
            false,
            0,
            0.0,
            0,
            false,
            new List<string>(),
            LenientBenchmarkProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBenchmarkProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBenchmarkProposalStatus next)
    {
        switch (Status)
        {
            case LenientBenchmarkProposalStatus.Planned:
                switch (next)
                {
                    case LenientBenchmarkProposalStatus.InProgress:
                    case LenientBenchmarkProposalStatus.Completed:
                        return true;
                }
                return false;
            case LenientBenchmarkProposalStatus.Scheduled:
                switch (next)
                {
                    case LenientBenchmarkProposalStatus.Completed:
                    case LenientBenchmarkProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientBenchmarkProposalStatus.InProgress:
                switch (next)
                {
                    case LenientBenchmarkProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientBenchmarkProposalStatus.Completed:
                switch (next)
                {
                    case LenientBenchmarkProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBenchmarkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient benchmark proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient benchmark proposal"} {Code}";
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

    public double EffectiveExposure()
    {
        return Math.Round(Budget + (double)EndsAt, 2);
    }

    public LenientBenchmarkProposal Copy()
    {
        return new LenientBenchmarkProposal(
            Code,
            Title,
            Channel,
            Active,
            GraceDays,
            Budget,
            EndsAt,
            Approved,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
