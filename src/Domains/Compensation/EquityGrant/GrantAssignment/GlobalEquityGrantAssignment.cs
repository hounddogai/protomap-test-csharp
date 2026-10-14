namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global equity grant assignment is in its life cycle.</summary>
public enum GlobalEquityGrantAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One global equity grant assignment, with what the planners need to act on it.</summary>
public sealed class GlobalEquityGrantAssignment(string code, string label, string grade, string contactEmail, int quantity, string checksum, double weight, double score, double discount, string salaryBand, string phone, GlobalEquityGrantAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Phone { get; set; } = phone;
    public GlobalEquityGrantAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a global equity grant assignment in status created.</summary>
    public static GlobalEquityGrantAssignment NewGlobalEquityGrantAssignment(string code, string label)
    {
        return new GlobalEquityGrantAssignment(
            code,
            label,
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            GlobalEquityGrantAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEquityGrantAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEquityGrantAssignmentStatus next)
    {
        switch (Status)
        {
            case GlobalEquityGrantAssignmentStatus.Created:
                switch (next)
                {
                    case GlobalEquityGrantAssignmentStatus.Published:
                    case GlobalEquityGrantAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case GlobalEquityGrantAssignmentStatus.Validated:
                switch (next)
                {
                    case GlobalEquityGrantAssignmentStatus.Published:
                    case GlobalEquityGrantAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case GlobalEquityGrantAssignmentStatus.Published:
                return next == GlobalEquityGrantAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalEquityGrantAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global equity grant assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"global equity grant assignment"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Quantity + Score, 2);
    }

    public GlobalEquityGrantAssignment Copy()
    {
        return new GlobalEquityGrantAssignment(
            Code,
            Label,
            Grade,
            ContactEmail,
            Quantity,
            Checksum,
            Weight,
            Score,
            Discount,
            SalaryBand,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
