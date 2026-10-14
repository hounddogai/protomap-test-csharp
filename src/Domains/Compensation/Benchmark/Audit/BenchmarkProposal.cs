namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark proposal is in its life cycle.</summary>
public enum BenchmarkProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A benchmark proposal. Its code is unique within a tenant.</summary>
public sealed class BenchmarkProposal(string code, string title, string phone, double rating, string locale, BenchmarkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public BenchmarkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark proposal in status idle.</summary>
    public static BenchmarkProposal NewBenchmarkProposal(string code, string title)
    {
        return new BenchmarkProposal(code, title, "", 0.0, "", BenchmarkProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkProposalStatus next)
    {
        switch (Status)
        {
            case BenchmarkProposalStatus.Idle:
                switch (next)
                {
                    case BenchmarkProposalStatus.Ready:
                        return true;
                }
                return false;
            case BenchmarkProposalStatus.Warming:
                return next == BenchmarkProposalStatus.Draining || next == BenchmarkProposalStatus.Stopped;
            case BenchmarkProposalStatus.Ready:
                return next == BenchmarkProposalStatus.Draining;
            case BenchmarkProposalStatus.Draining:
                switch (next)
                {
                    case BenchmarkProposalStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BenchmarkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"benchmark proposal"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public BenchmarkProposal Copy()
    {
        return new BenchmarkProposal(Code, Title, Phone, Rating, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
