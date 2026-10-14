namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception proposal is in its life cycle.</summary>
public enum ExceptionProposalStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A exception proposal as the exception team keeps it.</summary>
public sealed class ExceptionProposal(string code, string label, string department, string locale, double ceiling, int batchSize, bool pinned, int tenureMonths, string currency, int attempts, ExceptionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public ExceptionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a exception proposal in status idle.</summary>
    public static ExceptionProposal NewExceptionProposal(string code, string label)
    {
        return new ExceptionProposal(code, label, "", "", 0.0, 0, false, 0, "", 0, ExceptionProposalStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionProposalStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionProposalStatus next)
    {
        switch (Status)
        {
            case ExceptionProposalStatus.Idle:
                return next == ExceptionProposalStatus.Stopped;
            case ExceptionProposalStatus.Warming:
                return next == ExceptionProposalStatus.Draining || next == ExceptionProposalStatus.Stopped;
            case ExceptionProposalStatus.Ready:
                switch (next)
                {
                    case ExceptionProposalStatus.Draining:
                        return true;
                }
                return false;
            case ExceptionProposalStatus.Draining:
                switch (next)
                {
                    case ExceptionProposalStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"exception proposal"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Ceiling + (double)TenureMonths, 2);
    }

    public ExceptionProposal Copy()
    {
        return new ExceptionProposal(
            Code,
            Label,
            Department,
            Locale,
            Ceiling,
            BatchSize,
            Pinned,
            TenureMonths,
            Currency,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
