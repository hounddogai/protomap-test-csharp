namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip batch is in its life cycle.</summary>
public enum PayslipBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a payslip batch and its current status.</summary>
public sealed class PayslipBatch(string code, string title, string notes, List<string> tags, string name, string managerId, string country, double score, PayslipBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public PayslipBatchStatus Status { get; set; } = status;

    /// <summary>Starts a payslip batch in status idle.</summary>
    public static PayslipBatch NewPayslipBatch(string code, string title)
    {
        return new PayslipBatch(code, title, "", new List<string>(), "", "", "", 0.0, PayslipBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipBatchStatus next)
    {
        switch (Status)
        {
            case PayslipBatchStatus.Idle:
                return next == PayslipBatchStatus.Draining;
            case PayslipBatchStatus.Warming:
                switch (next)
                {
                    case PayslipBatchStatus.Ready:
                        return true;
                }
                return false;
            case PayslipBatchStatus.Ready:
                switch (next)
                {
                    case PayslipBatchStatus.Draining:
                        return true;
                }
                return false;
            case PayslipBatchStatus.Draining:
                return next == PayslipBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayslipBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public PayslipBatch Copy()
    {
        return new PayslipBatch(Code, Title, Notes, Tags, Name, ManagerId, Country, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
