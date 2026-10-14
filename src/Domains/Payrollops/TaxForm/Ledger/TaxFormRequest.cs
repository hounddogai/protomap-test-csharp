namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form request is in its life cycle.</summary>
public enum TaxFormRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tax form request, with what the planners need to act on it.</summary>
public sealed class TaxFormRequest(string code, string label, int createdAt, string region, string title, TaxFormRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public TaxFormRequestStatus Status { get; set; } = status;

    /// <summary>Starts a tax form request in status new.</summary>
    public static TaxFormRequest NewTaxFormRequest(string code, string label)
    {
        return new TaxFormRequest(code, label, 0, "", "", TaxFormRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormRequestStatus next)
    {
        switch (Status)
        {
            case TaxFormRequestStatus.New:
                switch (next)
                {
                    case TaxFormRequestStatus.Running:
                    case TaxFormRequestStatus.Done:
                        return true;
                }
                return false;
            case TaxFormRequestStatus.Queued:
                switch (next)
                {
                    case TaxFormRequestStatus.Done:
                    case TaxFormRequestStatus.Failed:
                        return true;
                }
                return false;
            case TaxFormRequestStatus.Running:
                switch (next)
                {
                    case TaxFormRequestStatus.Done:
                        return true;
                }
                return false;
            case TaxFormRequestStatus.Done:
                switch (next)
                {
                    case TaxFormRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tax form request"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public TaxFormRequest Copy()
    {
        return new TaxFormRequest(Code, Label, CreatedAt, Region, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
