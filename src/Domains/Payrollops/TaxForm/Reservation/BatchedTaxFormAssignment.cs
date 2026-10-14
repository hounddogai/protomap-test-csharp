namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched tax form assignment is in its life cycle.</summary>
public enum BatchedTaxFormAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched tax form assignment. Its code is unique within a tenant.</summary>
public sealed class BatchedTaxFormAssignment(string code, string title, double minimum, int leaveDays, double score, double maximum, string phone, int tenureMonths, int createdAt, string employeeId, int durationMinutes, BatchedTaxFormAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public double Score { get; set; } = score;
    public double Maximum { get; set; } = maximum;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public int CreatedAt { get; set; } = createdAt;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BatchedTaxFormAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a batched tax form assignment in status requested.</summary>
    public static BatchedTaxFormAssignment NewBatchedTaxFormAssignment(string code, string title)
    {
        return new BatchedTaxFormAssignment(
            code,
            title,
            0.0,
            0,
            0.0,
            0.0,
            "",
            0,
            0,
            "",
            0,
            BatchedTaxFormAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTaxFormAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTaxFormAssignmentStatus next)
    {
        switch (Status)
        {
            case BatchedTaxFormAssignmentStatus.Requested:
                switch (next)
                {
                    case BatchedTaxFormAssignmentStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedTaxFormAssignmentStatus.Granted:
                switch (next)
                {
                    case BatchedTaxFormAssignmentStatus.Revoked:
                    case BatchedTaxFormAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case BatchedTaxFormAssignmentStatus.Revoked:
                return next == BatchedTaxFormAssignmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedTaxFormAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched tax form assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched tax form assignment"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Score * (double)LeaveDays, 2);
    }

    public BatchedTaxFormAssignment Copy()
    {
        return new BatchedTaxFormAssignment(
            Code,
            Title,
            Minimum,
            LeaveDays,
            Score,
            Maximum,
            Phone,
            TenureMonths,
            CreatedAt,
            EmployeeId,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
