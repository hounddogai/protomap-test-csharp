namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form score is in its life cycle.</summary>
public enum TaxFormScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A tax form score as the tax form team keeps it.</summary>
public sealed class TaxFormScore(string code, string label, int batchSize, double weight, double cost, double hours, string employeeId, TaxFormScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public string EmployeeId { get; set; } = employeeId;
    public TaxFormScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tax form score in status new.</summary>
    public static TaxFormScore NewTaxFormScore(string code, string label)
    {
        return new TaxFormScore(code, label, 0, 0.0, 0.0, 0.0, "", TaxFormScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormScoreStatus next)
    {
        switch (Status)
        {
            case TaxFormScoreStatus.New:
                switch (next)
                {
                    case TaxFormScoreStatus.Done:
                    case TaxFormScoreStatus.Failed:
                        return true;
                }
                return false;
            case TaxFormScoreStatus.Queued:
                return next == TaxFormScoreStatus.Running;
            case TaxFormScoreStatus.Running:
                return next == TaxFormScoreStatus.Done || next == TaxFormScoreStatus.Failed;
            case TaxFormScoreStatus.Done:
                switch (next)
                {
                    case TaxFormScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tax form score"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Hours * (double)BatchSize, 2);
    }

    public TaxFormScore Copy()
    {
        return new TaxFormScore(Code, Label, BatchSize, Weight, Cost, Hours, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
