namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form assignment is in its life cycle.</summary>
public enum TaxFormAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tax form assignment and its current status.</summary>
public sealed class TaxFormAssignment(string code, string title, string phone, string category, int priority, double fte, string managerId, string checksum, double margin, string locale, string salaryBand, TaxFormAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public string ManagerId { get; set; } = managerId;
    public string Checksum { get; set; } = checksum;
    public double Margin { get; set; } = margin;
    public string Locale { get; set; } = locale;
    public string SalaryBand { get; set; } = salaryBand;
    public TaxFormAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a tax form assignment in status draft.</summary>
    public static TaxFormAssignment NewTaxFormAssignment(string code, string title)
    {
        return new TaxFormAssignment(
            code,
            title,
            "",
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            TaxFormAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormAssignmentStatus next)
    {
        switch (Status)
        {
            case TaxFormAssignmentStatus.Draft:
                switch (next)
                {
                    case TaxFormAssignmentStatus.Pending:
                    case TaxFormAssignmentStatus.Active:
                        return true;
                }
                return false;
            case TaxFormAssignmentStatus.Pending:
                switch (next)
                {
                    case TaxFormAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case TaxFormAssignmentStatus.Active:
                switch (next)
                {
                    case TaxFormAssignmentStatus.Suspended:
                    case TaxFormAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case TaxFormAssignmentStatus.Suspended:
                switch (next)
                {
                    case TaxFormAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Fte * (double)Priority, 2);
    }

    public TaxFormAssignment Copy()
    {
        return new TaxFormAssignment(
            Code,
            Title,
            Phone,
            Category,
            Priority,
            Fte,
            ManagerId,
            Checksum,
            Margin,
            Locale,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
