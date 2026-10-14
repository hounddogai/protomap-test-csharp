namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched booking template is in its life cycle.</summary>
public enum BatchedBookingTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a batched booking template and its current status.</summary>
public sealed class BatchedBookingTemplate(string code, string label, string jobCode, string salaryBand, bool pinned, double rate, bool approved, bool visible, bool archived, int updatedAt, BatchedBookingTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Pinned { get; set; } = pinned;
    public double Rate { get; set; } = rate;
    public bool Approved { get; set; } = approved;
    public bool Visible { get; set; } = visible;
    public bool Archived { get; set; } = archived;
    public int UpdatedAt { get; set; } = updatedAt;
    public BatchedBookingTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched booking template in status idle.</summary>
    public static BatchedBookingTemplate NewBatchedBookingTemplate(string code, string label)
    {
        return new BatchedBookingTemplate(
            code,
            label,
            "",
            "",
            false,
            0.0,
            false,
            false,
            false,
            0,
            BatchedBookingTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBookingTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBookingTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedBookingTemplateStatus.Idle:
                switch (next)
                {
                    case BatchedBookingTemplateStatus.Warming:
                        return true;
                }
                return false;
            case BatchedBookingTemplateStatus.Warming:
                return next == BatchedBookingTemplateStatus.Draining || next == BatchedBookingTemplateStatus.Stopped;
            case BatchedBookingTemplateStatus.Ready:
                return next == BatchedBookingTemplateStatus.Stopped;
            case BatchedBookingTemplateStatus.Draining:
                return next == BatchedBookingTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedBookingTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched booking template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"batched booking template"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)UpdatedAt * Rate, 2);
    }

    public BatchedBookingTemplate Copy()
    {
        return new BatchedBookingTemplate(
            Code,
            Label,
            JobCode,
            SalaryBand,
            Pinned,
            Rate,
            Approved,
            Visible,
            Archived,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
