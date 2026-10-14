namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan batch is in its life cycle.</summary>
public enum ReturnPlanBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One return plan batch, with what the planners need to act on it.</summary>
public sealed class ReturnPlanBatch(string code, string label, string title, string ownerId, int version, ReturnPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public ReturnPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a return plan batch in status requested.</summary>
    public static ReturnPlanBatch NewReturnPlanBatch(string code, string label)
    {
        return new ReturnPlanBatch(code, label, "", "", 0, ReturnPlanBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanBatchStatus next)
    {
        switch (Status)
        {
            case ReturnPlanBatchStatus.Requested:
                return next == ReturnPlanBatchStatus.Granted;
            case ReturnPlanBatchStatus.Granted:
                switch (next)
                {
                    case ReturnPlanBatchStatus.Expired:
                        return true;
                }
                return false;
            case ReturnPlanBatchStatus.Revoked:
                switch (next)
                {
                    case ReturnPlanBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public ReturnPlanBatch Copy()
    {
        return new ReturnPlanBatch(Code, Label, Title, OwnerId, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
