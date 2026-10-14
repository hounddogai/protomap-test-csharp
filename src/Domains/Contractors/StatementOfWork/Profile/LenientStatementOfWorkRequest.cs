namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient statement of work request is in its life cycle.</summary>
public enum LenientStatementOfWorkRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One lenient statement of work request, with what the planners need to act on it.</summary>
public sealed class LenientStatementOfWorkRequest(string code, string label, string source, string unit, string ownerId, string currency, int retries, string salaryBand, double discount, string segment, string location, LenientStatementOfWorkRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Unit { get; set; } = unit;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public int Retries { get; set; } = retries;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public LenientStatementOfWorkRequestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient statement of work request in status new.</summary>
    public static LenientStatementOfWorkRequest NewLenientStatementOfWorkRequest(string code, string label)
    {
        return new LenientStatementOfWorkRequest(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            "",
            0.0,
            "",
            "",
            LenientStatementOfWorkRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientStatementOfWorkRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientStatementOfWorkRequestStatus next)
    {
        switch (Status)
        {
            case LenientStatementOfWorkRequestStatus.New:
                return next == LenientStatementOfWorkRequestStatus.Done || next == LenientStatementOfWorkRequestStatus.Failed;
            case LenientStatementOfWorkRequestStatus.Queued:
                return next == LenientStatementOfWorkRequestStatus.Done || next == LenientStatementOfWorkRequestStatus.Failed;
            case LenientStatementOfWorkRequestStatus.Running:
                switch (next)
                {
                    case LenientStatementOfWorkRequestStatus.Done:
                        return true;
                }
                return false;
            case LenientStatementOfWorkRequestStatus.Done:
                switch (next)
                {
                    case LenientStatementOfWorkRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientStatementOfWorkRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient statement of work request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient statement of work request"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries - Discount, 2);
    }

    public LenientStatementOfWorkRequest Copy()
    {
        return new LenientStatementOfWorkRequest(
            Code,
            Label,
            Source,
            Unit,
            OwnerId,
            Currency,
            Retries,
            SalaryBand,
            Discount,
            Segment,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
