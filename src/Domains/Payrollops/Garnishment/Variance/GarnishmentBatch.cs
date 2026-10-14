namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment batch is in its life cycle.</summary>
public enum GarnishmentBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One garnishment batch, with what the planners need to act on it.</summary>
public sealed class GarnishmentBatch(string code, string title, double margin, int quantity, double maximum, string grade, GarnishmentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public GarnishmentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment batch in status planned.</summary>
    public static GarnishmentBatch NewGarnishmentBatch(string code, string title)
    {
        return new GarnishmentBatch(code, title, 0.0, 0, 0.0, "", GarnishmentBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentBatchStatus next)
    {
        switch (Status)
        {
            case GarnishmentBatchStatus.Planned:
                switch (next)
                {
                    case GarnishmentBatchStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentBatchStatus.Scheduled:
                return next == GarnishmentBatchStatus.InProgress;
            case GarnishmentBatchStatus.InProgress:
                switch (next)
                {
                    case GarnishmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentBatchStatus.Completed:
                switch (next)
                {
                    case GarnishmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment batch"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Quantity + Maximum, 2);
    }

    public GarnishmentBatch Copy()
    {
        return new GarnishmentBatch(Code, Title, Margin, Quantity, Maximum, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
