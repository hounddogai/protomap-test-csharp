namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped tax form score is in its life cycle.</summary>
public enum CappedTaxFormScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped tax form score. Its code is unique within a tenant.</summary>
public sealed class CappedTaxFormScore(string code, string title, int attempts, string contactEmail, string managerId, CappedTaxFormScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public CappedTaxFormScoreStatus Status { get; set; } = status;

    /// <summary>Starts a capped tax form score in status new.</summary>
    public static CappedTaxFormScore NewCappedTaxFormScore(string code, string title)
    {
        return new CappedTaxFormScore(code, title, 0, "", "", CappedTaxFormScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTaxFormScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTaxFormScoreStatus next)
    {
        switch (Status)
        {
            case CappedTaxFormScoreStatus.New:
                return next == CappedTaxFormScoreStatus.Failed;
            case CappedTaxFormScoreStatus.Queued:
                switch (next)
                {
                    case CappedTaxFormScoreStatus.Running:
                        return true;
                }
                return false;
            case CappedTaxFormScoreStatus.Running:
                return next == CappedTaxFormScoreStatus.Done || next == CappedTaxFormScoreStatus.Failed;
            case CappedTaxFormScoreStatus.Done:
                return next == CappedTaxFormScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedTaxFormScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped tax form score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped tax form score"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public CappedTaxFormScore Copy()
    {
        return new CappedTaxFormScore(Code, Title, Attempts, ContactEmail, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
